using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Messaging;
using MugenMvvm.Messaging;
using MugenMvvm.Messaging.Components;
using MugenMvvm.Tests.Internal;
using MugenMvvm.Tests.Messaging;
using MugenMvvm.UnitTests.Components;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Messaging
{
    public class MessengerTest : ComponentOwnerTestBase<Messenger>
    {
        [Fact]
        public void GetSubscribersShouldReturnEmptyListNoComponents() => Messenger.GetSubscribers(DefaultMetadata).AsList().ShouldBeEmpty();

        [Fact]
        public void TrySubscribeUnsubscribeUnsubscribeAllShouldNotifyListeners()
        {
            var invokedCount = 0;
            var hasCache = new TestHasCache { Invalidate = (_, _, _) => { ++invokedCount; } };
            Messenger.AddComponent(new MessengerHandlerSubscriber());
            Messenger.Components.TryAdd(hasCache);

            invokedCount.ShouldEqual(0);
            var handler = new TestMessengerHandler();
            Messenger.TrySubscribe(handler);
            invokedCount.ShouldEqual(1);

            Messenger.TryUnsubscribe(handler);
            invokedCount.ShouldEqual(2);

            Messenger.TrySubscribe(handler);
            invokedCount = 0;

            Messenger.UnsubscribeAll();
            invokedCount.ShouldEqual(1);

            Messenger.UnsubscribeAll();
            invokedCount.ShouldEqual(1);
        }

        protected override IMessenger GetMessenger() => GetComponentOwner(ComponentCollectionManager);

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void GetMessageContextShouldBeHandledByComponents(int count)
        {
            var sender = new object();
            var message = new object();
            var ctx = new MessageContext(sender, message, DefaultMetadata);
            var invokeCount = 0;
            for (var i = 0; i < count; i++)
            {
                var isLast = i == count - 1;
                Messenger.AddComponent(new TestMessageContextProviderComponent
                {
                    Priority = -i,
                    TryGetMessageContext = (m, o, o1, arg3) =>
                    {
                        ++invokeCount;
                        m.ShouldEqual(Messenger);
                        o.ShouldEqual(sender);
                        o1.ShouldEqual(message);
                        arg3.ShouldEqual(DefaultMetadata);
                        if (isLast)
                            return ctx;
                        return null;
                    }
                });
            }

            Messenger.GetMessageContext(sender, message, DefaultMetadata).ShouldEqual(ctx);
            invokeCount.ShouldEqual(count);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void PublishShouldBeHandledByComponents(int count)
        {
            var ctx = new MessageContext(new object(), new object(), DefaultMetadata);
            var invokeCount = 0;
            var result = false;
            for (var i = 0; i < count; i++)
            {
                Messenger.AddComponent(new TestMessagePublisherComponent
                {
                    Priority = -i,
                    TryPublish = (m, messageContext) =>
                    {
                        ++invokeCount;
                        m.ShouldEqual(Messenger);
                        messageContext.ShouldEqual(ctx);
                        return result;
                    }
                });
            }

            Messenger.Publish(ctx).ShouldEqual(result);
            invokeCount.ShouldEqual(count);

            invokeCount = 0;
            result = true;
            Messenger.Publish(ctx).ShouldEqual(result);
            invokeCount.ShouldEqual(count);
        }

        [Theory]
        [InlineData(1, null)]
        [InlineData(1, 1)]
        [InlineData(10, null)]
        [InlineData(10, 1)]
        public void SubscribeShouldBeHandledByComponents(int count, int? executionMode)
        {
            var threadMode = executionMode == null ? null : ThreadExecutionMode.Get(executionMode.Value);
            var invokeCount = 0;
            var result = false;
            for (var i = 0; i < count; i++)
            {
                Messenger.AddComponent(new TestMessengerSubscriberComponent
                {
                    Priority = -i,
                    TrySubscribe = (m, o, arg3, arg4) =>
                    {
                        ++invokeCount;
                        m.ShouldEqual(Messenger);
                        o.ShouldEqual(this);
                        arg3.ShouldEqual(threadMode);
                        arg4.ShouldEqual(DefaultMetadata);
                        return result;
                    }
                });
            }

            Messenger.TrySubscribe(this, threadMode, DefaultMetadata).ShouldEqual(result);
            invokeCount.ShouldEqual(count);

            invokeCount = 0;
            result = true;
            Messenger.TrySubscribe(this, threadMode, DefaultMetadata).ShouldEqual(result);
            invokeCount.ShouldEqual(count);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void UnsubscribeShouldBeHandledByComponents(int count)
        {
            var invokeCount = 0;
            var result = false;
            for (var i = 0; i < count; i++)
            {
                Messenger.AddComponent(new TestMessengerSubscriberComponent
                {
                    Priority = -i,
                    TryUnsubscribe = (m, o, arg3) =>
                    {
                        ++invokeCount;
                        m.ShouldEqual(Messenger);
                        o.ShouldEqual(this);
                        arg3.ShouldEqual(DefaultMetadata);
                        return result;
                    }
                });
            }

            Messenger.TryUnsubscribe(this, DefaultMetadata).ShouldEqual(result);
            invokeCount.ShouldEqual(count);

            invokeCount = 0;
            result = true;
            Messenger.TryUnsubscribe(this, DefaultMetadata).ShouldEqual(result);
            invokeCount.ShouldEqual(count);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void UnsubscribeAllShouldBeHandledByComponents(int count)
        {
            var invokeCount = 0;
            var result = false;
            for (var i = 0; i < count; i++)
            {
                Messenger.AddComponent(new TestMessengerSubscriberComponent
                {
                    Priority = -i,
                    TryUnsubscribeAll = (m, arg3) =>
                    {
                        ++invokeCount;
                        m.ShouldEqual(Messenger);
                        arg3.ShouldEqual(DefaultMetadata);
                        return result;
                    }
                });
            }

            Messenger.UnsubscribeAll(DefaultMetadata).ShouldEqual(result);
            invokeCount.ShouldEqual(count);

            invokeCount = 0;
            result = true;
            Messenger.UnsubscribeAll(DefaultMetadata).ShouldEqual(result);
            invokeCount.ShouldEqual(count);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void GetSubscribersShouldBeHandledByComponents(int count)
        {
            var subscribers = new HashSet<MessengerSubscriberInfo>();
            for (var i = 0; i < count; i++)
                subscribers.Add(new MessengerSubscriberInfo(new object(), ThreadExecutionMode.Background));
            for (var i = 0; i < count; i++)
            {
                var info = subscribers.ElementAt(i);
                Messenger.AddComponent(new TestMessengerSubscriberComponent
                {
                    Priority = -i,
                    TryGetSubscribers = (m, arg3) =>
                    {
                        m.ShouldEqual(Messenger);
                        arg3.ShouldEqual(DefaultMetadata);
                        return new[] { info };
                    }
                });
            }

            var result = Messenger.GetSubscribers(DefaultMetadata).AsList();
            result.Count.ShouldEqual(count);
            foreach (var messengerSubscriberInfo in result)
                subscribers.Remove(messengerSubscriberInfo);
            subscribers.Count.ShouldEqual(0);
        }

        protected override Messenger GetComponentOwner(IComponentCollectionManager? componentCollectionManager = null) => new(componentCollectionManager);
    }
}