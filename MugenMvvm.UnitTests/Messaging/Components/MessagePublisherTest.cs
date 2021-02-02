using System;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Messaging;
using MugenMvvm.Messaging.Components;
using MugenMvvm.Threading;
using MugenMvvm.UnitTests.Messaging.Internal;
using MugenMvvm.UnitTests.Threading.Internal;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Messaging.Components
{
    public class MessagePublisherTest : UnitTestBase
    {
        private readonly Messenger _messenger;
        private readonly MessagePublisher _messagePublisher;

        public MessagePublisherTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            _messenger = new Messenger(ComponentCollectionManager);
            _messagePublisher = new MessagePublisher(ThreadDispatcher);
            _messenger.AddComponent(_messagePublisher);
        }

        [Fact]
        public void TryPublishInvalidResultShouldRemoveSubscriber()
        {
            var invokedCount = 0;
            var handler = new MessengerHandler((o, o1, arg3) => MessengerResult.Invalid, this, ThreadExecutionMode.Current);
            var messengerHandlers = new[]
            {
                handler
            };
            var messageContext = new MessageContext(new object(), this, DefaultMetadata);
            var subscriberComponent = new TestMessengerSubscriberComponent(_messenger);
            _messenger.AddComponent(subscriberComponent);
            subscriberComponent.TryGetMessengerHandlers = (type, context) =>
            {
                type.ShouldEqual(messageContext.Message.GetType());
                context.ShouldEqual(DefaultMetadata);
                return messengerHandlers;
            };
            subscriberComponent.TryUnsubscribe = (o, arg3) =>
            {
                ++invokedCount;
                o.ShouldEqual(handler.Subscriber);
                return true;
            };

            _messenger.Publish(messageContext);
            invokedCount.ShouldEqual(1);
        }

        [Fact]
        public void TryPublishShouldCacheItems()
        {
            var invokedCount = 0;
            var tryGetMessengerHandlersCount = 0;
            var handler = new MessengerHandler((o, o1, arg3) =>
            {
                ++invokedCount;
                return MessengerResult.Handled;
            }, this, ThreadExecutionMode.Current);
            var messengerHandlers = new[]
            {
                handler
            };
            var messageContext = new MessageContext(new object(), this, DefaultMetadata);
            var subscriberComponent = new TestMessengerSubscriberComponent(_messenger);
            _messenger.AddComponent(subscriberComponent);
            subscriberComponent.TryGetMessengerHandlers = (type, context) =>
            {
                ++tryGetMessengerHandlersCount;
                type.ShouldEqual(messageContext.Message.GetType());
                context.ShouldEqual(DefaultMetadata);
                return messengerHandlers;
            };

            _messenger.Publish(messageContext);
            invokedCount.ShouldEqual(1);
            tryGetMessengerHandlersCount.ShouldEqual(1);

            _messenger.Publish(messageContext);
            invokedCount.ShouldEqual(2);
            tryGetMessengerHandlersCount.ShouldEqual(1);

            _messenger.TryInvalidateCache();

            _messenger.Publish(messageContext);
            invokedCount.ShouldEqual(3);
            tryGetMessengerHandlersCount.ShouldEqual(2);

            _messenger.Publish(messageContext);
            invokedCount.ShouldEqual(4);
            tryGetMessengerHandlersCount.ShouldEqual(2);

            _messenger.RemoveComponent(_messagePublisher);
            _messenger.AddComponent(_messagePublisher);

            _messenger.Publish(messageContext);
            invokedCount.ShouldEqual(5);
            tryGetMessengerHandlersCount.ShouldEqual(3);

            _messenger.Publish(messageContext);
            invokedCount.ShouldEqual(6);
            tryGetMessengerHandlersCount.ShouldEqual(3);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void TryPublishShouldUseTryGetMessengerHandlers(int count)
        {
            var messageContext = new MessageContext(new object(), this, DefaultMetadata);
            var subscriberComponent = new TestMessengerSubscriberComponent(_messenger);
            _messenger.AddComponent(subscriberComponent);

            var result = MessengerResult.Handled;
            var invokedCount = 0;
            var messengerHandlers = new MessengerHandler[count];
            for (var i = 0; i < messengerHandlers.Length; i++)
            {
                messengerHandlers[i] = new MessengerHandler((o, arg3, o1) =>
                {
                    ++invokedCount;
                    messageContext.ShouldEqual(arg3);
                    return result;
                }, this, ThreadExecutionMode.Current);
            }

            subscriberComponent.TryGetMessengerHandlers = (type, context) =>
            {
                type.ShouldEqual(messageContext.Message.GetType());
                context.ShouldEqual(DefaultMetadata);
                return messengerHandlers;
            };

            _messenger.Publish(messageContext);
            invokedCount.ShouldEqual(count);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(-1)]
        public void TryPublishShouldUseThreadDispatcher(int threadMode)
        {
            ThreadExecutionMode.TryGet(threadMode, out var threadExecutionMode);
            var invokedCount = 0;
            var messengerHandlers = new[]
            {
                new MessengerHandler((o, o1, arg3) =>
                {
                    ++invokedCount;
                    return MessengerResult.Handled;
                }, this, threadExecutionMode)
            };
            var messageContext = new MessageContext(new object(), this, DefaultMetadata);
            var threadDispatcher = new ThreadDispatcher(ComponentCollectionManager);
            var component = new MessagePublisher(threadDispatcher);

            if (threadExecutionMode == null)
                threadExecutionMode = component.DefaultExecutionMode;

            Action? invokeAction = null;
            var testThreadDispatcherComponent = new TestThreadDispatcherComponent
            {
                CanExecuteInline = (_, __) => false,
                Execute = (action, mode, arg3, _) =>
                {
                    mode.ShouldEqual(threadExecutionMode);
                    invokeAction = () => action(arg3);
                    return true;
                }
            };
            using var t = threadDispatcher.AddComponent(testThreadDispatcherComponent);
            _messenger.ClearComponents();
            var subscriberComponent = new TestMessengerSubscriberComponent(_messenger);
            _messenger.AddComponent(component);
            _messenger.AddComponent(subscriberComponent);
            subscriberComponent.TryGetMessengerHandlers = (type, context) =>
            {
                type.ShouldEqual(messageContext.Message.GetType());
                context.ShouldEqual(DefaultMetadata);
                return messengerHandlers;
            };

            component.TryPublish(_messenger, messageContext);
            invokedCount.ShouldEqual(0);
            invokeAction.ShouldNotBeNull();

            invokeAction?.Invoke();
            invokedCount.ShouldEqual(1);
        }
    }
}