using System;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Messaging;
using MugenMvvm.Messaging;
using MugenMvvm.Messaging.Components;
using MugenMvvm.Tests.Messaging;
using MugenMvvm.Tests.Threading;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Messaging.Components
{
    public class MessagePublisherTest : UnitTestBase
    {
        private readonly MessagePublisher _messagePublisher;

        public MessagePublisherTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            _messagePublisher = new MessagePublisher(ThreadDispatcher);
            Messenger.AddComponent(_messagePublisher);
        }

        [Fact]
        public void TryPublishInvalidResultShouldRemoveSubscriber()
        {
            var invokedCount = 0;
            var handler = new MessengerHandler((_, _, _) => MessengerResult.Invalid, this, ThreadExecutionMode.Current);
            var messengerHandlers = new[]
            {
                handler
            };
            var messageContext = new MessageContext(new object(), this, DefaultMetadata);
            Messenger.AddComponent(new TestMessengerSubscriberComponent
            {
                TryGetMessengerHandlers = (m, type, context) =>
                {
                    m.ShouldEqual(Messenger);
                    type.ShouldEqual(messageContext.Message.GetType());
                    context.ShouldEqual(DefaultMetadata);
                    return messengerHandlers;
                },
                TryUnsubscribe = (m, o, _) =>
                {
                    ++invokedCount;
                    m.ShouldEqual(Messenger);
                    o.ShouldEqual(handler.Subscriber);
                    return true;
                }
            });

            Messenger.Publish(messageContext);
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
            Messenger.AddComponent(new TestMessengerSubscriberComponent
            {
                TryGetMessengerHandlers = (m, type, context) =>
                {
                    ++tryGetMessengerHandlersCount;
                    m.ShouldEqual(Messenger);
                    type.ShouldEqual(messageContext.Message.GetType());
                    context.ShouldEqual(DefaultMetadata);
                    return messengerHandlers;
                }
            });

            Messenger.Publish(messageContext);
            invokedCount.ShouldEqual(1);
            tryGetMessengerHandlersCount.ShouldEqual(1);

            Messenger.Publish(messageContext);
            invokedCount.ShouldEqual(2);
            tryGetMessengerHandlersCount.ShouldEqual(1);

            Messenger.TryInvalidateCache();

            Messenger.Publish(messageContext);
            invokedCount.ShouldEqual(3);
            tryGetMessengerHandlersCount.ShouldEqual(2);

            Messenger.Publish(messageContext);
            invokedCount.ShouldEqual(4);
            tryGetMessengerHandlersCount.ShouldEqual(2);

            Messenger.RemoveComponent(_messagePublisher);
            Messenger.AddComponent(_messagePublisher);

            Messenger.Publish(messageContext);
            invokedCount.ShouldEqual(5);
            tryGetMessengerHandlersCount.ShouldEqual(3);

            Messenger.Publish(messageContext);
            invokedCount.ShouldEqual(6);
            tryGetMessengerHandlersCount.ShouldEqual(3);
        }

        protected override IMessenger GetMessenger() => new Messenger(ComponentCollectionManager);

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void TryPublishShouldUseTryGetMessengerHandlers(int count)
        {
            var messageContext = new MessageContext(new object(), this, DefaultMetadata);

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

            Messenger.AddComponent(new TestMessengerSubscriberComponent
            {
                TryGetMessengerHandlers = (m, type, context) =>
                {
                    m.ShouldEqual(Messenger);
                    type.ShouldEqual(messageContext.Message.GetType());
                    context.ShouldEqual(DefaultMetadata);
                    return messengerHandlers;
                }
            });

            Messenger.Publish(messageContext);
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
                new MessengerHandler((_, _, _) =>
                {
                    ++invokedCount;
                    return MessengerResult.Handled;
                }, this, threadExecutionMode)
            };
            var messageContext = new MessageContext(new object(), this, DefaultMetadata);
            if (threadExecutionMode == null)
                threadExecutionMode = _messagePublisher.DefaultExecutionMode;

            Action? invokeAction = null;
            ThreadDispatcher.ClearComponents();
            ThreadDispatcher.AddComponent(new TestThreadDispatcherComponent
            {
                CanExecuteInline = (_, _, __) => false,
                Execute = (_, action, mode, arg3, _) =>
                {
                    mode.ShouldEqual(threadExecutionMode);
                    invokeAction = () => action(arg3);
                    return true;
                }
            });
            Messenger.ClearComponents();

            Messenger.AddComponent(new TestMessengerSubscriberComponent
            {
                TryGetMessengerHandlers = (m, type, context) =>
                {
                    m.ShouldEqual(Messenger);
                    type.ShouldEqual(messageContext.Message.GetType());
                    context.ShouldEqual(DefaultMetadata);
                    return messengerHandlers;
                }
            });

            _messagePublisher.TryPublish(Messenger, messageContext);
            invokedCount.ShouldEqual(0);
            invokeAction.ShouldNotBeNull();

            invokeAction?.Invoke();
            invokedCount.ShouldEqual(1);
        }
    }
}