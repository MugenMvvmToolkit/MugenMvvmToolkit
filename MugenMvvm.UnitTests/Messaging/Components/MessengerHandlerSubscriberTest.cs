using System;
using System.Collections.Generic;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Messaging;
using MugenMvvm.Messaging;
using MugenMvvm.Messaging.Components;
using MugenMvvm.UnitTests.Messaging.Internal;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Messaging.Components
{
    public class MessengerHandlerSubscriberTest : UnitTestBase
    {
        private readonly Messenger _messenger;
        private readonly MessengerHandlerSubscriber _messengerHandlerComponent;

        public MessengerHandlerSubscriberTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            _messenger = new Messenger(ComponentCollectionManager);
            _messengerHandlerComponent = new MessengerHandlerSubscriber(ReflectionManager);
            _messenger.AddComponent(_messengerHandlerComponent);
        }

        [Fact]
        public void HandleShouldReturnInvalidResultTargetIsNotAlive()
        {
            var invokedCount = 0;
            var handler = new TestMessengerHandler
            {
                HandleString = (s, context) => { ++invokedCount; }
            };
            var weakRef = handler.ToWeakReference();
            _messenger.TrySubscribe(weakRef, null, null);

            var handlers = _messengerHandlerComponent.TryGetMessengerHandlers(_messenger, typeof(string), null)!.AsList();
            weakRef.Release();
            handlers[0].Handle(new MessageContext(this, "")).ShouldEqual(MessengerResult.Invalid);
        }

        [Fact]
        public void TryGetMessengerHandlersShouldUnsubscribeIfSubscriberIsNotAlive()
        {
            var handler = new TestMessengerHandler();
            var weakRef = handler.ToWeakReference();
            _messenger.TrySubscribe(weakRef, null, null);

            _messengerHandlerComponent.TryGetMessengerHandlers(_messenger, typeof(string), null)!.AsList().Count.ShouldEqual(1);
            weakRef.Release();
            _messengerHandlerComponent.TryGetMessengerHandlers(_messenger, typeof(string), null).AsList().ShouldBeEmpty();
        }

        [Fact]
        public void TrySubscribeUnsubscribeShouldReturnFalseNotSupported()
        {
            _messenger.TrySubscribe(this, null, DefaultMetadata).ShouldBeFalse();
            _messenger.GetSubscribers(null).AsList().ShouldBeEmpty();
            _messenger.TryUnsubscribe(this, DefaultMetadata).ShouldBeFalse();
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void TrySubscribeUnsubscribeGetAllTest(int count)
        {
            var hashSet = new HashSet<MessengerSubscriberInfo>();
            for (var i = 0; i < count; i++)
            {
                var handler = new TestMessengerHandler();
                ThreadExecutionMode.TryGet(i, out var mode);
                _messenger.TrySubscribe(handler, mode, DefaultMetadata).ShouldBeTrue();

                var info = new MessengerSubscriberInfo(handler, mode);
                hashSet.Add(info);
            }

            var subscribers = _messenger.GetSubscribers(DefaultMetadata)!.AsList();
            subscribers.Count.ShouldEqual(hashSet.Count);
            foreach (var messengerSubscriberInfo in subscribers)
                hashSet.Remove(messengerSubscriberInfo).ShouldBeTrue();
            hashSet.Count.ShouldEqual(0);

            foreach (var messengerSubscriberInfo in subscribers)
                _messenger.TryUnsubscribe(messengerSubscriberInfo.Subscriber!, DefaultMetadata).ShouldBeTrue();
            _messenger.GetSubscribers(DefaultMetadata).AsList().ShouldBeEmpty();
        }

        [Theory]
        [InlineData(1, true)]
        [InlineData(1, false)]
        [InlineData(10, true)]
        [InlineData(10, false)]
        public void TrySubscribeUnsubscribeGetAllTestWeakReference(int count, bool keepAlive)
        {
            var list = new List<object>();
            var hashSet = new HashSet<MessengerSubscriberInfo>();
            for (var i = 0; i < count; i++)
            {
                var handler = new TestMessengerHandler().ToWeakReference();
                if (keepAlive)
                    list.Add(handler.Target!);
                ThreadExecutionMode.TryGet(i, out var mode);
                _messenger.TrySubscribe(handler, mode, DefaultMetadata).ShouldBeTrue();
                var info = new MessengerSubscriberInfo(handler, mode);
                hashSet.Add(info);
            }

            var subscribers = _messenger.GetSubscribers(DefaultMetadata)!.AsList();
            subscribers.Count.ShouldEqual(hashSet.Count);
            foreach (var messengerSubscriberInfo in subscribers)
                hashSet.Remove(messengerSubscriberInfo).ShouldBeTrue();
            hashSet.Count.ShouldEqual(0);

            GcCollect();

            foreach (var messengerSubscriberInfo in subscribers)
                _messenger.TryUnsubscribe(messengerSubscriberInfo.Subscriber!, DefaultMetadata).ShouldBeTrue();
            _messenger.GetSubscribers(DefaultMetadata).AsList().ShouldBeEmpty();
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void TryUnsubscribeAllShouldRemoveAllSubscribers(int count)
        {
            for (var i = 0; i < count; i++)
            {
                var handler = new TestMessengerHandler();
                _messenger.TrySubscribe(handler, ThreadExecutionMode.TryGet(i % 4, ThreadExecutionMode.Background), DefaultMetadata).ShouldBeTrue();
            }

            _messengerHandlerComponent.TryUnsubscribeAll(_messenger, DefaultMetadata);
            _messenger.GetSubscribers(DefaultMetadata).AsList().ShouldBeEmpty();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void TryGetMessengerHandlersShouldReturnHandlers1(bool isWeak)
        {
            const string stringMessage = "test";
            const int intMessage = 1;
            var invokedCount = 0;
            var invokedStringCount = 0;
            IMessageContext? ctx = null;

            invokedCount.ShouldEqual(0);
            var handler = new TestMessengerHandler
            {
                HandleString = (s, context) =>
                {
                    ++invokedStringCount;
                    context.ShouldEqual(ctx);
                    s.ShouldEqual(stringMessage);
                },
                HandleInt = (i, context) =>
                {
                    ++invokedCount;
                    context.ShouldEqual(ctx);
                    i.ShouldEqual(intMessage);
                }
            };
            if (isWeak)
                _messenger.TrySubscribe(handler.ToWeakReference(), ThreadExecutionMode.Current, null);
            else
                _messenger.TrySubscribe(handler, ThreadExecutionMode.Current, null);

            ctx = new MessageContext(this, intMessage, DefaultMetadata);
            var handlers = _messengerHandlerComponent.TryGetMessengerHandlers(_messenger, typeof(int), DefaultMetadata)!.AsList();
            handlers.Count.ShouldEqual(1);
            handlers[0].ExecutionMode.ShouldEqual(ThreadExecutionMode.Current);
            handlers[0].Handle(ctx).ShouldEqual(MessengerResult.Handled);
            invokedCount.ShouldEqual(1);
            invokedStringCount.ShouldEqual(0);

            ctx = new MessageContext(this, stringMessage, DefaultMetadata);
            handlers = _messengerHandlerComponent.TryGetMessengerHandlers(_messenger, typeof(string), DefaultMetadata)!.AsList();
            handlers.Count.ShouldEqual(1);
            handlers[0].ExecutionMode.ShouldEqual(ThreadExecutionMode.Current);
            handlers[0].Handle(ctx).ShouldEqual(MessengerResult.Handled);
            invokedCount.ShouldEqual(1);
            invokedStringCount.ShouldEqual(1);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void TryGetMessengerHandlersShouldReturnHandlers2(bool isWeak)
        {
            const string stringMessage = "test";
            const int intMessage = 1;
            var invokedCount = 0;
            IMessageContext? ctx = null;

            invokedCount.ShouldEqual(0);
            var handler = new TestMessengerHandlerGeneric<object>
            {
                Handle = (i, t, context) =>
                {
                    ++invokedCount;
                    context.ShouldEqual(ctx);
                    i.ShouldEqual(context.Message);
                }
            };
            if (isWeak)
                _messenger.TrySubscribe(handler.ToWeakReference(), ThreadExecutionMode.Current, null);
            else
                _messenger.TrySubscribe(handler, ThreadExecutionMode.Current, null);

            ctx = new MessageContext(this, intMessage, DefaultMetadata);
            var handlers = _messengerHandlerComponent.TryGetMessengerHandlers(_messenger, typeof(int), DefaultMetadata)!.AsList();
            handlers.Count.ShouldEqual(1);
            handlers[0].ExecutionMode.ShouldEqual(ThreadExecutionMode.Current);
            handlers[0].Handle(ctx).ShouldEqual(MessengerResult.Handled);
            invokedCount.ShouldEqual(1);

            ctx = new MessageContext(this, stringMessage, DefaultMetadata);
            handlers = _messengerHandlerComponent.TryGetMessengerHandlers(_messenger, typeof(string), DefaultMetadata)!.AsList();
            handlers.Count.ShouldEqual(1);
            handlers[0].ExecutionMode.ShouldEqual(ThreadExecutionMode.Current);
            handlers[0].Handle(ctx).ShouldEqual(MessengerResult.Handled);
            invokedCount.ShouldEqual(2);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void TryGetMessengerHandlersShouldReturnHandlers3(bool isWeak)
        {
            const string stringMessage = "test";
            const int intMessage = 1;
            var invokedCount = 0;
            Type? canHandleType = null;
            IMessageContext? ctx = null;

            invokedCount.ShouldEqual(0);
            var handler = new TestMessengerHandlerRaw
            {
                CanHandle = type =>
                {
                    canHandleType = type;
                    return true;
                },
                Handle = context =>
                {
                    ++invokedCount;
                    context.ShouldEqual(ctx);
                    return MessengerResult.Handled;
                }
            };
            if (isWeak)
                _messenger.TrySubscribe(handler.ToWeakReference(), ThreadExecutionMode.Current, null);
            else
                _messenger.TrySubscribe(handler, ThreadExecutionMode.Current, null);

            ctx = new MessageContext(this, intMessage, DefaultMetadata);
            var handlers = _messengerHandlerComponent.TryGetMessengerHandlers(_messenger, typeof(int), DefaultMetadata)!.AsList();
            canHandleType.ShouldEqual(typeof(int));
            handlers.Count.ShouldEqual(1);
            handlers[0].ExecutionMode.ShouldEqual(ThreadExecutionMode.Current);
            handlers[0].Handle(ctx).ShouldEqual(MessengerResult.Handled);
            invokedCount.ShouldEqual(1);

            ctx = new MessageContext(this, stringMessage, DefaultMetadata);
            handlers = _messengerHandlerComponent.TryGetMessengerHandlers(_messenger, typeof(string), DefaultMetadata)!.AsList();
            canHandleType.ShouldEqual(typeof(string));
            handlers.Count.ShouldEqual(1);
            handlers[0].ExecutionMode.ShouldEqual(ThreadExecutionMode.Current);
            handlers[0].Handle(ctx).ShouldEqual(MessengerResult.Handled);
            invokedCount.ShouldEqual(2);
        }
    }
}