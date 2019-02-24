using System.Collections.Generic;
using System.Threading.Tasks;
using MugenMvvm.Attributes;
using MugenMvvm.Collections;
using MugenMvvm.Enums;
using MugenMvvm.Infrastructure.Internal;
using MugenMvvm.Infrastructure.Metadata;
using MugenMvvm.Interfaces.Messaging;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Threading;

namespace MugenMvvm.Infrastructure.Messaging
{
    public class Messenger : HasListenersBase<IMessengerListener>, IMessenger, IEqualityComparer<KeyValuePair<ThreadExecutionMode, IMessengerSubscriber>>
    {
        #region Fields

        private readonly HashSet<KeyValuePair<ThreadExecutionMode, IMessengerSubscriber>> _subscribers;
        private readonly IThreadDispatcher _threadDispatcher;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public Messenger(IThreadDispatcher threadDispatcher)
        {
            Should.NotBeNull(threadDispatcher, nameof(threadDispatcher));
            _threadDispatcher = threadDispatcher;
            _subscribers = new HashSet<KeyValuePair<ThreadExecutionMode, IMessengerSubscriber>>(this);
        }

        #endregion

        #region Implementation of interfaces

        bool IEqualityComparer<KeyValuePair<ThreadExecutionMode, IMessengerSubscriber>>.Equals(KeyValuePair<ThreadExecutionMode, IMessengerSubscriber> x,
            KeyValuePair<ThreadExecutionMode, IMessengerSubscriber> y)
        {
            return x.Value.Equals(y.Value);
        }

        int IEqualityComparer<KeyValuePair<ThreadExecutionMode, IMessengerSubscriber>>.GetHashCode(KeyValuePair<ThreadExecutionMode, IMessengerSubscriber> obj)
        {
            return obj.Value.GetHashCode();
        }

        public IReadOnlyList<MessengerSubscriberInfo> GetSubscribers()
        {
            var index = 0;
            lock (_subscribers)
            {
                var subscribers = new MessengerSubscriberInfo[_subscribers.Count];
                foreach (var subscriber in _subscribers)
                    subscribers[index++] = new MessengerSubscriberInfo(subscriber.Value, subscriber.Key);
                return subscribers;
            }
        }

        IMessengerContext IMessenger.GetContext(IMetadataContext? metadata)
        {
            return GetContext(metadata);
        }

        public void Publish(object sender, object message, IMessengerContext? messengerContext = null)
        {
            PublishInternalAsync(sender, message, messengerContext, false);
        }

        public Task PublishAsync(object sender, object message, IMessengerContext? messengerContext = null)
        {
            return PublishInternalAsync(sender, message, messengerContext, true);
        }

        public void Subscribe(IMessengerSubscriber subscriber, ThreadExecutionMode executionMode)
        {
            Should.NotBeNull(subscriber, nameof(subscriber));
            Should.NotBeNull(executionMode, nameof(executionMode));
            bool added;
            lock (_subscribers)
            {
                added = _subscribers.Add(new KeyValuePair<ThreadExecutionMode, IMessengerSubscriber>(executionMode, subscriber));
            }

            if (added)
            {
                var listeners = GetListenersInternal();
                if (listeners == null)
                    return;
                for (var i = 0; i < listeners.Length; i++)
                    (listeners[i] as IMessengerListener)?.OnSubscribed(this, subscriber, executionMode);
            }
        }

        public bool Unsubscribe(IMessengerSubscriber subscriber)
        {
            Should.NotBeNull(subscriber, nameof(subscriber));
            bool removed;
            lock (_subscribers)
            {
                removed = _subscribers.Remove(new KeyValuePair<ThreadExecutionMode, IMessengerSubscriber>(ThreadExecutionMode.Current, subscriber));
            }

            if (removed)
            {
                var listeners = GetListenersInternal();
                if (listeners == null)
                    return true;

                for (var i = 0; i < listeners.Length; i++)
                    (listeners[i] as IMessengerListener)?.OnUnsubscribed(this, subscriber);
            }

            return removed;
        }

        #endregion

        #region Methods

        private MessengerContext GetContext(IMetadataContext? metadata)
        {
            var ctx = new MessengerContext(this, metadata);
            var listeners = GetListenersInternal();
            if (listeners != null)
            {
                for (var i = 0; i < listeners.Length; i++)
                    (listeners[i] as IMessengerListener)?.OnContextCreated(this, ctx);
            }

            return ctx;
        }

        private Task PublishInternalAsync(object sender, object message, IMessengerContext? messengerContext, bool isAsync)
        {
            Should.NotBeNull(sender, nameof(sender));
            Should.NotBeNull(message, nameof(message));
            MessengerContext? rawContext = null;
            if (messengerContext == null)
            {
                rawContext = GetContext(null);
                messengerContext = rawContext;
            }

            var dictionary = new Dictionary<ThreadExecutionMode, ThreadDispatcherExecutor>();
            lock (_subscribers)
            {
                foreach (var subscriber in _subscribers)
                {
                    if (rawContext == null)
                    {
                        if (!messengerContext.MarkAsHandled(subscriber.Value))
                            continue;
                    }
                    else
                    {
                        if (!rawContext.MarkAsHandledNoLock(subscriber.Value))
                            continue;
                    }

                    if (!dictionary.TryGetValue(subscriber.Key, out var value))
                    {
                        value = new ThreadDispatcherExecutor(this, sender, message, messengerContext);
                        dictionary[subscriber.Key] = value;
                    }

                    value.Add(subscriber.Value);
                }
            }

            if (isAsync)
            {
                var tasks = new Task[dictionary.Count];
                int index = 0;
                foreach (var dispatcherExecutor in dictionary)
                {
                    tasks[index] = _threadDispatcher.ExecuteAsync(dispatcherExecutor.Value, dispatcherExecutor.Key, null);
                    ++index;
                }
                return Task.WhenAll(tasks);
            }

            foreach (var dispatcherExecutor in dictionary)
                _threadDispatcher.Execute(dispatcherExecutor.Value, dispatcherExecutor.Key, null);
            return Default.CompletedTask;
        }

        #endregion

        #region Nested types

        private sealed class ThreadDispatcherExecutor : LightArrayList<IMessengerSubscriber>, IThreadDispatcherHandler
        {
            #region Fields

            private readonly IMessengerContext _context;
            private readonly object _message;
            private readonly Messenger _messenger;
            private readonly object _sender;

            #endregion

            #region Constructors

            public ThreadDispatcherExecutor(Messenger messenger, object sender, object message, IMessengerContext context)
            {
                _messenger = messenger;
                _sender = sender;
                _message = message;
                _context = context;
            }

            #endregion

            #region Implementation of interfaces

            public void Execute(object? state)
            {
                var subscribers = GetItems(out var size);
                if (_messenger.HasListeners)
                {
                    for (var i = 0; i < size; i++)
                        PublishAndNotify(subscribers[i]);
                }
                else
                {
                    for (var i = 0; i < size; i++)
                    {
                        if (subscribers[i].Handle(_sender, _message, _context) == MessengerSubscriberResult.Invalid)
                            _messenger.Unsubscribe(subscribers[i]);
                    }
                }
            }

            #endregion

            #region Methods

            private void PublishAndNotify(IMessengerSubscriber subscriber)
            {
                var listeners = _messenger.GetListenersInternal();
                if (listeners == null)
                {
                    if (subscriber.Handle(_sender, _message, _context) == MessengerSubscriberResult.Invalid)
                        _messenger.Unsubscribe(subscriber);
                    return;
                }

                MessengerSubscriberResult? subscriberResult = null;
                for (var i = 0; i < listeners.Length; i++)
                {
                    subscriberResult = listeners[i]?.OnPublishing(_messenger, subscriber, _sender, _message, _context);
                    if (subscriberResult != null)
                        break;
                }

                var result = subscriberResult ?? subscriber.Handle(_sender, _message, _context);

                for (var i = 0; i < listeners.Length; i++)
                    listeners[i]?.OnPublished(_messenger, subscriber, _sender, _message, _context, result);

                if (result == MessengerSubscriberResult.Invalid)
                    _messenger.Unsubscribe(subscriber);
            }

            #endregion
        }

        private sealed class MessengerContext : HashSet<object>, IMessengerContext
        {
            #region Fields

            private IMetadataContext? _metadata;

            #endregion

            #region Constructors

            public MessengerContext(IMessenger messenger, IMetadataContext? metadata)
            {
                Messenger = messenger;
                _metadata = metadata;
            }

            #endregion

            #region Properties

            public IMetadataContext Metadata
            {
                get
                {
                    if (_metadata == null)
                        MugenExtensions.LazyInitialize(ref _metadata, new MetadataContext());
                    return _metadata!;
                }
            }

            public IMessenger Messenger { get; }

            #endregion

            #region Implementation of interfaces

            public bool MarkAsHandled(object handler)
            {
                lock (this)
                {
                    return Add(handler);
                }
            }

            #endregion

            #region Methods

            public bool MarkAsHandledNoLock(object handler)
            {
                return Add(handler);
            }

            #endregion
        }

        #endregion
    }
}