using System;
using System.Collections.Generic;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Messaging;
using MugenMvvm.Messaging;
using MugenMvvm.Messaging.Components;
using MugenMvvm.Tests.Messaging;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Messaging.Components
{
    public class MessengerHandlerSubscriberTest : UnitTestBase
    {
        private readonly MessengerHandlerSubscriber _messengerHandlerComponent;

        public MessengerHandlerSubscriberTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            _messengerHandlerComponent = new MessengerHandlerSubscriber(ReflectionManager);
            Messenger.AddComponent(_messengerHandlerComponent);
        }

        [Fact]
        public void HandleShouldReturnInvalidResultTargetIsNotAlive()
        {
            var invokedCount = 0;
            var handler = new TestMessengerHandler
            {
                HandleString = (_, _) => { ++invokedCount; }
            };
            var weakRef = handler.ToWeakReference(WeakReferenceManager);
            Messenger.TrySubscribe(weakRef);

            var handlers = _messengerHandlerComponent.TryGetMessengerHandlers(Messenger, typeof(string), null)!;
            weakRef.Release();
            handlers[0].Handle(new MessageContext(this, "")).ShouldEqual(MessengerResult.Invalid);
            invokedCount.ShouldEqual(0);
        }

        [Fact]
        public void TryGetMessengerHandlersShouldUnsubscribeIfSubscriberIsNotAlive()
        {
            var handler = new TestMessengerHandler();
            var weakRef = handler.ToWeakReference(WeakReferenceManager);
            Messenger.TrySubscribe(weakRef);

            _messengerHandlerComponent.TryGetMessengerHandlers(Messenger, typeof(string), null)!.Count.ShouldEqual(1);
            weakRef.Release();
            _messengerHandlerComponent.TryGetMessengerHandlers(Messenger, typeof(string), null).ShouldBeEmpty();
        }

        [Fact]
        public void TrySubscribeUnsubscribeShouldReturnFalseNotSupported()
        {
            Messenger.TrySubscribe(this, null, DefaultMetadata).ShouldBeFalse();
            Messenger.GetSubscribers().ShouldBeEmpty();
            Messenger.TryUnsubscribe(this, DefaultMetadata).ShouldBeFalse();
        }

        protected override IMessenger GetMessenger() => new Messenger(ComponentCollectionManager);

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
                Messenger.TrySubscribe(handler, mode, DefaultMetadata).ShouldBeTrue();

                var info = new MessengerSubscriberInfo(handler, mode);
                hashSet.Add(info);
            }

            var subscribers = Messenger.GetSubscribers(DefaultMetadata)!;
            subscribers.Count.ShouldEqual(hashSet.Count);
            foreach (var messengerSubscriberInfo in subscribers)
                hashSet.Remove(messengerSubscriberInfo).ShouldBeTrue();
            hashSet.Count.ShouldEqual(0);

            foreach (var messengerSubscriberInfo in subscribers)
                Messenger.TryUnsubscribe(messengerSubscriberInfo.Subscriber!, DefaultMetadata).ShouldBeTrue();
            Messenger.GetSubscribers(DefaultMetadata).ShouldBeEmpty();
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
                var handler = new TestMessengerHandler().ToWeakReference(WeakReferenceManager);
                if (keepAlive)
                    list.Add(handler.Target!);
                ThreadExecutionMode.TryGet(i, out var mode);
                Messenger.TrySubscribe(handler, mode, DefaultMetadata).ShouldBeTrue();
                var info = new MessengerSubscriberInfo(handler, mode);
                hashSet.Add(info);
            }

            var subscribers = Messenger.GetSubscribers(DefaultMetadata)!;
            subscribers.Count.ShouldEqual(hashSet.Count);
            foreach (var messengerSubscriberInfo in subscribers)
                hashSet.Remove(messengerSubscriberInfo).ShouldBeTrue();
            hashSet.Count.ShouldEqual(0);

            GcCollect();

            foreach (var messengerSubscriberInfo in subscribers)
                Messenger.TryUnsubscribe(messengerSubscriberInfo.Subscriber!, DefaultMetadata).ShouldBeTrue();
            Messenger.GetSubscribers(DefaultMetadata).ShouldBeEmpty();
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void TryUnsubscribeAllShouldRemoveAllSubscribers(int count)
        {
            for (var i = 0; i < count; i++)
                Messenger.TrySubscribe(new TestMessengerHandler(), ThreadExecutionMode.TryGet(i % 4, ThreadExecutionMode.Background), DefaultMetadata).ShouldBeTrue();

            _messengerHandlerComponent.TryUnsubscribeAll(Messenger, DefaultMetadata);
            Messenger.GetSubscribers(DefaultMetadata).ShouldBeEmpty();
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
                Messenger.TrySubscribe(handler.ToWeakReference(WeakReferenceManager), ThreadExecutionMode.Current);
            else
                Messenger.TrySubscribe(handler, ThreadExecutionMode.Current);

            ctx = new MessageContext(this, intMessage, DefaultMetadata);
            var handlers = _messengerHandlerComponent.TryGetMessengerHandlers(Messenger, typeof(int), DefaultMetadata)!;
            handlers.Count.ShouldEqual(1);
            handlers[0].ExecutionMode.ShouldEqual(ThreadExecutionMode.Current);
            handlers[0].Handle(ctx).ShouldEqual(MessengerResult.Handled);
            invokedCount.ShouldEqual(1);
            invokedStringCount.ShouldEqual(0);

            ctx = new MessageContext(this, stringMessage, DefaultMetadata);
            handlers = _messengerHandlerComponent.TryGetMessengerHandlers(Messenger, typeof(string), DefaultMetadata)!;
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
                Messenger.TrySubscribe(handler.ToWeakReference(WeakReferenceManager), ThreadExecutionMode.Current);
            else
                Messenger.TrySubscribe(handler, ThreadExecutionMode.Current);

            ctx = new MessageContext(this, intMessage, DefaultMetadata);
            var handlers = _messengerHandlerComponent.TryGetMessengerHandlers(Messenger, typeof(int), DefaultMetadata)!;
            handlers.Count.ShouldEqual(1);
            handlers[0].ExecutionMode.ShouldEqual(ThreadExecutionMode.Current);
            handlers[0].Handle(ctx).ShouldEqual(MessengerResult.Handled);
            invokedCount.ShouldEqual(1);

            ctx = new MessageContext(this, stringMessage, DefaultMetadata);
            handlers = _messengerHandlerComponent.TryGetMessengerHandlers(Messenger, typeof(string), DefaultMetadata)!;
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
                Messenger.TrySubscribe(handler.ToWeakReference(WeakReferenceManager), ThreadExecutionMode.Current);
            else
                Messenger.TrySubscribe(handler, ThreadExecutionMode.Current);

            ctx = new MessageContext(this, intMessage, DefaultMetadata);
            var handlers = _messengerHandlerComponent.TryGetMessengerHandlers(Messenger, typeof(int), DefaultMetadata)!;
            canHandleType.ShouldEqual(typeof(int));
            handlers.Count.ShouldEqual(1);
            handlers[0].ExecutionMode.ShouldEqual(ThreadExecutionMode.Current);
            handlers[0].Handle(ctx).ShouldEqual(MessengerResult.Handled);
            invokedCount.ShouldEqual(1);

            ctx = new MessageContext(this, stringMessage, DefaultMetadata);
            handlers = _messengerHandlerComponent.TryGetMessengerHandlers(Messenger, typeof(string), DefaultMetadata)!;
            canHandleType.ShouldEqual(typeof(string));
            handlers.Count.ShouldEqual(1);
            handlers[0].ExecutionMode.ShouldEqual(ThreadExecutionMode.Current);
            handlers[0].Handle(ctx).ShouldEqual(MessengerResult.Handled);
            invokedCount.ShouldEqual(2);
        }
    }
}