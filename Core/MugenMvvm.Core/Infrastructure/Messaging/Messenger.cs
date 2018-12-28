using System;
using System.Collections.Generic;
using MugenMvvm.Attributes;
using MugenMvvm.Collections;
using MugenMvvm.Infrastructure.Metadata;
using MugenMvvm.Interfaces;
using MugenMvvm.Interfaces.Messaging;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Threading;
using MugenMvvm.Models;

namespace MugenMvvm.Infrastructure.Messaging
{
    public class Messenger : IMessenger, IEqualityComparer<KeyValuePair<ThreadExecutionMode, IMessengerSubscriber>>
    {
        #region Fields

        private readonly HashSet<KeyValuePair<ThreadExecutionMode, IMessengerSubscriber>> _subscribers;
        private readonly IThreadDispatcher _threadDispatcher;
        private readonly ITracer _tracer;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)] 
        public Messenger(IThreadDispatcher threadDispatcher, ITracer tracer)
        {
            Should.NotBeNull(threadDispatcher, nameof(threadDispatcher));
            Should.NotBeNull(tracer, nameof(tracer));
            _threadDispatcher = threadDispatcher;
            _tracer = tracer;
            _subscribers = new HashSet<KeyValuePair<ThreadExecutionMode, IMessengerSubscriber>>(this);
        }

        #endregion

        #region Properties

        public Func<object, object, IMessengerContext, bool>? IsTraceableMessage { get; set; }

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

        public IMessengerContext GetContext(IMetadataContext? metadata)
        {
            return new MessengerContext(metadata);
        }

        public void Publish(object sender, object message, IMessengerContext? messengerContext = null)
        {
            Should.NotBeNull(sender, nameof(sender));
            Should.NotBeNull(message, nameof(message));
            if (messengerContext == null)
                messengerContext = new MessengerContext(null);
            var dictionary = new Dictionary<ThreadExecutionMode, ThreadDispatcherExecutor>();
            lock (_subscribers)
            {
                foreach (var subscriber in _subscribers)
                {
                    if (!messengerContext.MarkAsHandled(subscriber.Value))
                        continue;

                    if (!dictionary.TryGetValue(subscriber.Key, out var value))
                    {
                        value = new ThreadDispatcherExecutor(this, sender, message, messengerContext);
                        dictionary[subscriber.Key] = value;
                    }
                    value.Add(subscriber.Value);
                }
            }

            foreach (var dispatcherExecutor in dictionary)
                _threadDispatcher.Execute(dispatcherExecutor.Value, dispatcherExecutor.Key, null);
        }

        public void Subscribe(IMessengerSubscriber subscriber, ThreadExecutionMode? executionMode = null)
        {
            Should.NotBeNull(subscriber, nameof(subscriber));
            lock (_subscribers)
            {
                _subscribers.Add(new KeyValuePair<ThreadExecutionMode, IMessengerSubscriber>(executionMode ?? ThreadExecutionMode.Current, subscriber));
            }
        }

        public void Unsubscribe(IMessengerSubscriber subscriber)
        {
            Should.NotBeNull(subscriber, nameof(subscriber));
            lock (_subscribers)
            {
                _subscribers.Remove(new KeyValuePair<ThreadExecutionMode, IMessengerSubscriber>(ThreadExecutionMode.Current, subscriber));
            }
        }

        #endregion

        #region Nested types

        private sealed class ThreadDispatcherExecutor : ArrayListLight<IMessengerSubscriber>, IThreadDispatcherHandler
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
                var trace = _message is ITraceableMessage || _messenger.IsTraceableMessage != null && _messenger.IsTraceableMessage(_sender, _message, _context);
                int size;
                var subscribers = GetItems(out size);
                for (var i = 0; i < size; i++)
                {
                    var result = subscribers[i]?.Handle(_sender, _message, _context);
                    if (result == SubscriberResult.Handled)
                    {
                        if (trace)
                            _messenger._tracer.Warn(MessageConstants.MessageSentFromToFormat3.Format(_message, _sender, subscribers[i]));
                    }
                    else if (result == SubscriberResult.Invalid)
                        _messenger.Unsubscribe(subscribers[i]);
                }
            }

            #endregion            
        }

        private sealed class MessengerContext : HashSet<object>, IMessengerContext
        {
            #region Fields

            private IMetadataContext? _metadata;

            #endregion

            #region Constructors

            public MessengerContext(IMetadataContext? metadata)
            {
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
        }

        #endregion
    }
}