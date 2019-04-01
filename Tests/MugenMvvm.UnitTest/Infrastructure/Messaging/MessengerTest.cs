using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MugenMvvm.Enums;
using MugenMvvm.Infrastructure.Messaging;
using MugenMvvm.Infrastructure.Metadata;
using MugenMvvm.Interfaces.Messaging;
using MugenMvvm.Interfaces.Threading;
using MugenMvvm.UnitTest.TestInfrastructure;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Infrastructure.Messaging
{
    public class MessengerTest : UnitTestBase//todo add listeners test
    {
        #region Methods

        [Fact]
        public void MessengerShouldValidateArgsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new Messenger(null!));
        }

        [Theory]
        [MemberData(nameof(GetExecutionModes))]
        public void SubscribeShouldAddSubscriber(ThreadExecutionMode executionMode)
        {
            var subscriber = new TestSubscriber();
            var messenger = CreateMessenger();
            messenger.Subscribe(subscriber, executionMode);
            var single = messenger.GetSubscribers().Single();
            single.Subscriber.ShouldEqual(subscriber);
            single.ExecutionMode.ShouldEqual(executionMode);
        }

        [Fact]
        public void SubscribeShouldAddSubscriberOnce()
        {
            var subscriber = new TestSubscriber();
            var messenger = CreateMessenger();
            messenger.Subscribe(subscriber, ThreadExecutionMode.Current);
            messenger.Subscribe(subscriber, ThreadExecutionMode.Current);
            messenger.GetSubscribers().Single().Subscriber.ShouldEqual(subscriber);
        }

        [Fact]
        public void UnsubscribeShouldRemoveSubscriber()
        {
            var subscriber = new TestSubscriber();
            var messenger = CreateMessenger();
            messenger.Subscribe(subscriber, ThreadExecutionMode.Current);
            messenger.GetSubscribers().Single().Subscriber.ShouldEqual(subscriber);

            messenger.Unsubscribe(subscriber);
            messenger.GetSubscribers().ShouldBeEmpty();
        }

        [Fact]
        public void MessengerShouldUpdateSubscribers()
        {
            var messenger = CreateMessenger();
            var subscribers = new List<IMessengerSubscriber>();
            for (var i = 0; i < 100; i++)
                subscribers.Add(new TestSubscriber());

            for (var i = 0; i < subscribers.Count; i++)
                messenger.Subscribe(subscribers[i], ThreadExecutionMode.Current);

            for (var i = 0; i < subscribers.Count / 2; i++)
            {
                messenger.Unsubscribe(subscribers[i]);
                subscribers.RemoveAt(0);
                --i;
            }

            var list = messenger.GetSubscribers().Select(info => info.Subscriber).ToList();
            foreach (var subscriber in subscribers)
                list.ShouldContain(subscriber);
        }

        [Fact]
        public void MessengerShouldUpdateSubscribersParallel()
        {
            var messenger = CreateMessenger();
            var subscribers = new List<IMessengerSubscriber>();
            for (var i = 0; i < 100; i++)
                subscribers.Add(new TestSubscriber());

            var tasks = new List<Task>();
            for (var i = 0; i < subscribers.Count; i++)
            {
                var index = i;
                tasks.Add(Task.Run(() => messenger.Subscribe(subscribers[index], ThreadExecutionMode.Current)));
            }

            Task.WaitAll(tasks.ToArray());
            tasks.Clear();

            for (var i = 0; i < subscribers.Count / 2; i++)
            {
                var i1 = i;
                tasks.Add(Task.Run(() => messenger.Unsubscribe(subscribers[i1])));
            }

            var list = messenger.GetSubscribers().Select(info => info.Subscriber).ToList();
            foreach (var listener in subscribers.Skip(subscribers.Count / 2))
                list.ShouldContain(listener);
        }

        [Fact]
        public void UnsubscribeAllShouldRemoveAllSubscribers()
        {
            var subscriber1 = new TestSubscriber();
            var subscriber2 = new TestSubscriber();
            var messenger = CreateMessenger();
            messenger.Subscribe(subscriber1, ThreadExecutionMode.Current);
            messenger.Subscribe(subscriber2, ThreadExecutionMode.Current);
            messenger.GetSubscribers().Select(info => info.Subscriber).ShouldContain(subscriber1, subscriber2);

            messenger.UnsubscribeAll();
            messenger.GetSubscribers().ShouldBeEmpty();
        }

        [Fact]
        public void GetContextShouldCreateContextNullMetadata()
        {
            var messenger = CreateMessenger();
            var messengerContext = messenger.GetMessengerContext(null);
            messengerContext.Metadata.ShouldNotBeNull();
        }

        [Fact]
        public void GetContextShouldCreateContextNotNullMetadata()
        {
            var metadata = new MetadataContext();
            var messenger = CreateMessenger();
            var messengerContext = messenger.GetMessengerContext(metadata);
            messengerContext.Metadata.ShouldEqual(metadata);
        }

        [Fact]
        public void MarkAsHandledShouldNotAllowDuplicates()
        {
            var messenger = CreateMessenger();
            var messengerContext = messenger.GetMessengerContext(null);
            messengerContext.MarkAsHandled(messenger).ShouldBeTrue();
            messengerContext.MarkAsHandled(messenger).ShouldBeFalse();
        }

        [Fact]
        public void MarkAsHandledShouldBeThreadSafe()
        {
            const int count = 1000;
            var messenger = CreateMessenger();
            var messengerContext = messenger.GetMessengerContext(null);

            var objects = new object[count];
            for (var i = 0; i < objects.Length; i++)
                objects[i] = new object();

            var tasks = new List<Task>();
            for (var i = 0; i < count; i++)
            {
                var o = objects[i];
                tasks.Add(Task.Run(() => messengerContext.MarkAsHandled(o).ShouldBeTrue()));
            }

            Task.WaitAll(tasks.ToArray());

            foreach (var o in objects)
                messengerContext.MarkAsHandled(o).ShouldBeFalse();
        }

        [Theory]
        [MemberData(nameof(GetExecutionModes))]
        public void PublishShouldNotifySubscriberWithThreadExecutionMode(ThreadExecutionMode mode)
        {
            object? sender = null;
            object? message = null;
            IMessengerContext? context = null;
            var listener = new TestSubscriber
            {
                HandleDelegate = (o, o1, arg3) =>
                {
                    sender = o;
                    message = o1;
                    context = arg3;
                    return MessengerSubscriberResult.Handled;
                }
            };
            ThreadExecutionMode? executedMode = null;
            var dispatcher = new TestThreadDispatcher
            {
                ExecuteAction = (action, executionMode, arg3, ctx) =>
                {
                    action(arg3);
                    executedMode = executionMode;
                },
                ExecuteHandler = (action, executionMode, arg3, ctx) =>
                {
                    action.Execute(arg3);
                    executedMode = executionMode;
                }
            };

            var messenger = CreateMessenger(dispatcher);
            messenger.Subscribe(listener, mode);

            messenger.Publish(this, messenger);
            listener.Count.ShouldEqual(1);
            sender.ShouldEqual(this);
            messenger.ShouldEqual(messenger);
            mode.ShouldEqual(executedMode);
        }

        [Theory]
        [MemberData(nameof(GetPublishShouldUnsubscribeAfterInvalidResultArgs))]
        public void PublishShouldUnsubscribeInvalidResult(MessengerSubscriberResult subscriberResult, int result)
        {
            var listener = new TestSubscriber
            {
                HandleDelegate = (o, o1, arg3) => subscriberResult
            };
            var messenger = CreateMessenger();
            messenger.Subscribe(listener, ThreadExecutionMode.Current);
            messenger.Publish(this, messenger);
            messenger.GetSubscribers().Count.ShouldEqual(result);
        }

        [Fact]
        public void MessengerShouldAvoidLooping()
        {
            var messenger1 = CreateMessenger();
            var messenger2 = CreateMessenger();
            messenger1.Subscribe(new MessengerRepublisherSubscriber(messenger2), ThreadExecutionMode.Current);
            messenger2.Subscribe(new MessengerRepublisherSubscriber(messenger1), ThreadExecutionMode.Current);
            messenger1.Publish(messenger1, new object());
        }

        [Fact]
        public void MessengerShouldAvoidLoopingParallel()
        {
            var m1 = CreateMessenger();
            var m2 = CreateMessenger();
            var m3 = CreateMessenger();

            m1.Subscribe(new MessengerRepublisherSubscriber(m2), ThreadExecutionMode.Current);
            m1.Subscribe(new MessengerRepublisherSubscriber(m3), ThreadExecutionMode.Current);
            m2.Subscribe(new MessengerRepublisherSubscriber(m1), ThreadExecutionMode.Current);
            m2.Subscribe(new MessengerRepublisherSubscriber(m3), ThreadExecutionMode.Current);
            m3.Subscribe(new MessengerRepublisherSubscriber(m2), ThreadExecutionMode.Current);
            m3.Subscribe(new MessengerRepublisherSubscriber(m3), ThreadExecutionMode.Current);

            var subscribers = new List<TestSubscriber>();
            for (var i = 0; i < 1000; i++)
            {
                var subscriber = new TestSubscriber { HandleDelegate = (o, o1, arg3) => MessengerSubscriberResult.Handled };
                subscribers.Add(subscriber);
                m1.Subscribe(subscriber, ThreadExecutionMode.Current);

                subscriber = new TestSubscriber { HandleDelegate = (o, o1, arg3) => MessengerSubscriberResult.Handled };
                subscribers.Add(subscriber);
                m2.Subscribe(subscriber, ThreadExecutionMode.Current);

                subscriber = new TestSubscriber { HandleDelegate = (o, o1, arg3) => MessengerSubscriberResult.Handled };
                subscribers.Add(subscriber);
                m3.Subscribe(subscriber, ThreadExecutionMode.Current);
            }

            const int count = 1000;
            var messages = new HashSet<object>();
            for (var i = 0; i < count; i++)
                messages.Add(new object());

            var tasks = new List<Task>();
            for (var i = 0; i < messages.Count; i++)
            {
                var o = messages.ElementAt(i);
                tasks.Add(Task.Run(() => m1.Publish(o, o)));
            }

            Task.WaitAll(tasks.ToArray());

            foreach (var testSubscriber in subscribers)
            {
                testSubscriber.Messages.Count.ShouldEqual(messages.Count);
                messages.IsSupersetOf(testSubscriber.Messages).ShouldBeTrue();
            }
        }

        protected virtual IMessenger CreateMessenger(IThreadDispatcher? threadDispatcher = null)
        {
            if (threadDispatcher == null)
                threadDispatcher = new TestThreadDispatcher();
            return new Messenger(threadDispatcher);
        }

        public static IEnumerable<object[]> GetPublishShouldUnsubscribeAfterInvalidResultArgs()
        {
            return new[]
            {
                new object[] {MessengerSubscriberResult.Handled, 1},
                new object[] {MessengerSubscriberResult.Invalid, 0},
                new object[] {MessengerSubscriberResult.Ignored, 1}
            };
        }

        public static IEnumerable<object[]> GetExecutionModes()
        {
            return new[]
            {
                new[] {ThreadExecutionMode.Background},
                new[] {ThreadExecutionMode.Current},
                new[] {ThreadExecutionMode.Main},
                new[] {new ThreadExecutionMode(100)}
            };
        }

        #endregion

        #region Nested types

        private sealed class MessengerRepublisherSubscriber : IMessengerSubscriber
        {
            #region Fields

            private readonly IMessenger _messenger;

            #endregion

            #region Constructors

            public MessengerRepublisherSubscriber(IMessenger messenger)
            {
                _messenger = messenger;
            }

            #endregion

            #region Implementation of interfaces

            public bool Equals(IMessengerSubscriber other)
            {
                return ReferenceEquals(other, this);
            }

            public MessengerSubscriberResult Handle(object sender, object message, IMessengerContext messengerContext)
            {
                _messenger.Publish(sender, message, messengerContext);
                return MessengerSubscriberResult.Handled;
            }

            #endregion
        }

        private sealed class TestSubscriber : IMessengerSubscriber
        {
            #region Properties

            public int Count => Messages.Count;

            public Func<IMessengerSubscriber, IMessengerSubscriber, bool>? EqualsDelegate { get; set; }

            public Func<IMessengerSubscriber, int>? GetHashCodeDelegate { get; set; }

            public Func<object, object, IMessengerContext, MessengerSubscriberResult>? HandleDelegate { get; set; }

            public List<object> Messages { get; } = new List<object>();

            #endregion

            #region Implementation of interfaces

            public bool Equals(IMessengerSubscriber other)
            {
                return EqualsDelegate?.Invoke(this, other) ?? ReferenceEquals(this, other);
            }

            public MessengerSubscriberResult Handle(object sender, object message, IMessengerContext messengerContext)
            {
                lock (Messages)
                {
                    Messages.Add(message);
                }

                return HandleDelegate!(sender, message, messengerContext);
            }

            #endregion

            #region Methods

            public override int GetHashCode()
            {
                return GetHashCodeDelegate?.Invoke(this) ?? base.GetHashCode();
            }

            public override bool Equals(object obj)
            {
                if (obj is TestSubscriber testSubscriber)
                    return Equals(testSubscriber);
                return base.Equals(obj);
            }

            #endregion
        }

        #endregion
    }
}