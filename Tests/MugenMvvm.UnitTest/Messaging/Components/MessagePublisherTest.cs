using System;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Threading;
using MugenMvvm.Messaging;
using MugenMvvm.Messaging.Components;
using MugenMvvm.Threading;
using MugenMvvm.UnitTest.Internal.Internal;
using MugenMvvm.UnitTest.Messaging.Internal;
using MugenMvvm.UnitTest.Threading.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Messaging.Components
{
    public class MessagePublisherTest : UnitTestBase
    {
        #region Methods

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void TryPublishShouldUseTryGetMessengerHandlers(int count)
        {
            var messageContext = new MessageContext(new object(), this, DefaultMetadata);
            var component = new MessagePublisher();
            var messenger = new Messenger();
            var subscriberComponent = new TestMessengerSubscriberComponent();
            messenger.AddComponent(component);
            messenger.AddComponent(subscriberComponent);

            var result = MessengerResult.Handled;
            var invokedCount = 0;
            var messengerHandlers = new MessengerHandler[count];
            for (var i = 0; i < messengerHandlers.Length; i++)
            {
                messengerHandlers[i] = new MessengerHandler((o, o1, arg3) =>
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

            component.TryPublish(messageContext);
            invokedCount.ShouldEqual(count);
        }

        [Theory]
        [InlineData(true, 1)]
        [InlineData(true, 2)]
        [InlineData(false, 1)]
        [InlineData(false, 2)]
        public void TryPublishShouldUseThreadDispatcher(bool global, int threadMode)
        {
            var threadExecutionMode = ThreadExecutionMode.Parse(threadMode);
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
            IThreadDispatcher threadDispatcher;
            MessagePublisher component;
            if (global)
            {
                threadDispatcher = MugenService.ThreadDispatcher;
                component = new MessagePublisher();
            }
            else
            {
                threadDispatcher = new ThreadDispatcher();
                component = new MessagePublisher(threadDispatcher);
            }

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
            using var subscriber = TestComponentSubscriber.Subscribe(threadDispatcher, testThreadDispatcherComponent);
            var messenger = new Messenger();
            var subscriberComponent = new TestMessengerSubscriberComponent();
            messenger.AddComponent(component);
            messenger.AddComponent(subscriberComponent);
            subscriberComponent.TryGetMessengerHandlers = (type, context) =>
            {
                type.ShouldEqual(messageContext.Message.GetType());
                context.ShouldEqual(DefaultMetadata);
                return messengerHandlers;
            };

            component.TryPublish(messageContext);
            invokedCount.ShouldEqual(0);
            invokeAction.ShouldNotBeNull();

            invokeAction?.Invoke();
            invokedCount.ShouldEqual(1);
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
            var component = new MessagePublisher();
            var messenger = new Messenger();
            var subscriberComponent = new TestMessengerSubscriberComponent();
            messenger.AddComponent(component);
            messenger.AddComponent(subscriberComponent);
            subscriberComponent.TryGetMessengerHandlers = (type, context) =>
            {
                type.ShouldEqual(messageContext.Message.GetType());
                context.ShouldEqual(DefaultMetadata);
                return messengerHandlers;
            };
            subscriberComponent.TryUnsubscribe = (o, type, arg3) =>
            {
                ++invokedCount;
                o.ShouldEqual(handler);
                type.ShouldEqual(handler.GetType());
                return true;
            };

            component.TryPublish(messageContext);
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
            var component = new MessagePublisher();
            var messenger = new Messenger();
            var subscriberComponent = new TestMessengerSubscriberComponent();
            messenger.AddComponent(component);
            messenger.AddComponent(subscriberComponent);
            subscriberComponent.TryGetMessengerHandlers = (type, context) =>
            {
                ++tryGetMessengerHandlersCount;
                type.ShouldEqual(messageContext.Message.GetType());
                context.ShouldEqual(DefaultMetadata);
                return messengerHandlers;
            };

            component.TryPublish(messageContext);
            invokedCount.ShouldEqual(1);
            tryGetMessengerHandlersCount.ShouldEqual(1);

            component.TryPublish(messageContext);
            invokedCount.ShouldEqual(2);
            tryGetMessengerHandlersCount.ShouldEqual(1);

            component.Invalidate<object?>(null, null);

            component.TryPublish(messageContext);
            invokedCount.ShouldEqual(3);
            tryGetMessengerHandlersCount.ShouldEqual(2);

            component.TryPublish(messageContext);
            invokedCount.ShouldEqual(4);
            tryGetMessengerHandlersCount.ShouldEqual(2);

            messenger.RemoveComponent(component);
            messenger.AddComponent(component);

            component.TryPublish(messageContext);
            invokedCount.ShouldEqual(5);
            tryGetMessengerHandlersCount.ShouldEqual(3);

            component.TryPublish(messageContext);
            invokedCount.ShouldEqual(6);
            tryGetMessengerHandlersCount.ShouldEqual(3);
        }

        #endregion
    }
}