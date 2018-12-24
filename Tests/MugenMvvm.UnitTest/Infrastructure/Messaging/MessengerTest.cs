using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MugenMvvm.Infrastructure;
using MugenMvvm.Infrastructure.Messaging;
using MugenMvvm.Infrastructure.Metadata;
using MugenMvvm.Interfaces;
using MugenMvvm.Interfaces.Messaging;
using MugenMvvm.Models;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Infrastructure.Messaging
{
    public class MessengerTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void SubscribeShouldAddListener()
        {
            var item = new TestSubscriber();
            var messenger = CreateMessenger(Singleton<IThreadDispatcher>.Instance);
            messenger.Subscribe(item);
            messenger.GetSubscribers().ShouldContain(item);
        }

        [Fact]
        public void UnsubscribeShouldRemoveListener()
        {
            var listener = new TestSubscriber();
            var messenger = CreateMessenger(Singleton<IThreadDispatcher>.Instance);
            messenger.Subscribe(listener);
            messenger.GetSubscribers().ShouldContain(listener);

            messenger.Unsubscribe(listener);
        }

        [Fact]
        public void MessengerShouldUpdateSubscribers()
        {
            var messenger = CreateMessenger(Singleton<IThreadDispatcher>.Instance);
            var subscribers = new List<IMessengerSubscriber>();
            for (var i = 0; i < 100; i++)
                subscribers.Add(new TestSubscriber());

            for (var i = 0; i < subscribers.Count; i++)
                messenger.Subscribe(subscribers[i]);

            for (var i = 0; i < subscribers.Count / 2; i++)
            {
                messenger.Unsubscribe(subscribers[i]);
                subscribers.RemoveAt(0);
                --i;
            }

            var list = messenger.GetSubscribers();
            foreach (var listener in subscribers)
                list.ShouldContain(listener);
        }

        [Fact]
        public void MessengerShouldUpdateSubscribersParallel()
        {
            var messenger = CreateMessenger(Singleton<IThreadDispatcher>.Instance);
            var subscribers = new List<IMessengerSubscriber>();
            for (var i = 0; i < 100; i++)
                subscribers.Add(new TestSubscriber());

            var tasks = new List<Task>();
            for (var i = 0; i < subscribers.Count; i++)
            {
                var index = i;
                tasks.Add(Task.Run(() => messenger.Subscribe(subscribers[index])));
            }

            Task.WaitAll(tasks.ToArray());
            tasks.Clear();

            for (var i = 0; i < subscribers.Count / 2; i++)
            {
                var i1 = i;
                tasks.Add(Task.Run(() => messenger.Unsubscribe(subscribers[i1])));
            }

            var list = messenger.GetSubscribers();
            foreach (var listener in subscribers.Skip(subscribers.Count / 2))
                list.ShouldContain(listener);
        }

        [Fact]
        public void UnsubscribeAllShouldRemoveAllSubscribers()
        {
            var subscriber1 = new TestSubscriber();
            var subscriber2 = new TestSubscriber();
            var messenger = new Messenger(Singleton<IThreadDispatcher>.Instance);
            messenger.Subscribe(subscriber1);
            messenger.Subscribe(subscriber2);
            messenger.GetSubscribers().ShouldContain(subscriber1, subscriber2);

            messenger.UnsubscribeAll();
            messenger.GetSubscribers().ShouldBeEmpty();
        }

        [Fact]
        public void GetContextShouldCreateContextNullMetadata()
        {
            var messenger = new Messenger(Singleton<IThreadDispatcher>.Instance);
            var messengerContext = messenger.GetContext(null);
            messengerContext.Metadata.ShouldNotBeNull();
        }

        [Fact]
        public void GetContextShouldCreateContextNotNullMetadata()
        {
            var context = new MetadataContext();
            var messenger = new Messenger(Singleton<IThreadDispatcher>.Instance);
            var messengerContext = messenger.GetContext(context);
            messengerContext.Metadata.ShouldEqual(context);
        }

        protected virtual IMessenger CreateMessenger(IThreadDispatcher threadDispatcher)
        {
            return new Messenger(threadDispatcher);
        }

        #endregion

        #region Nested types

        private sealed class TestSubscriber : IMessengerSubscriber
        {
            #region Properties

            public Func<IMessengerSubscriber, IMessengerSubscriber, bool>? EqualsDelegate { get; set; }

            public Func<IMessengerSubscriber, int>? GetHashCodeDelegate { get; set; }

            #endregion

            #region Implementation of interfaces

            public bool Equals(IMessengerSubscriber other)
            {
                return EqualsDelegate?.Invoke(this, other) ?? ReferenceEquals(this, other);
            }

            public SubscriberResult Handle(object sender, object message, IMessengerContext messengerContext)
            {
                throw new NotImplementedException();
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