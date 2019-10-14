using System.Collections.Generic;
using System.Linq;
using System.Threading;
using MugenMvvm.Collections;
using MugenMvvm.Collections.Internal;
using MugenMvvm.Components;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Messaging;
using MugenMvvm.Interfaces.Messaging.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Threading;

namespace MugenMvvm.Messaging
{
    public sealed class Messenger : ComponentOwnerBase<IMessenger>, IMessenger, IEqualityComparer<MessengerSubscriberInfo>,
        IComponentOwnerAddedCallback<IComponent<IMessenger>>, IComponentOwnerRemovedCallback<IComponent<IMessenger>>
    {
        #region Fields

        private readonly TypeLightDictionary<ThreadExecutionModeDictionary?> _cache;
        private readonly IMetadataContextProvider? _metadataContextProvider;

        private readonly HashSet<MessengerSubscriberInfo> _subscribers;
        private readonly IThreadDispatcher? _threadDispatcher;
        private IMessageContextProviderComponent[] _contextProviders;
        private IMessengerHandlerComponent[] _handlerComponents;
        private IMessengerHandlerListener[] _listeners;

        #endregion

        #region Constructors

        public Messenger(IThreadDispatcher? threadDispatcher = null,
            IComponentCollectionProvider? componentCollectionProvider = null,
            IMetadataContextProvider? metadataContextProvider = null) : base(componentCollectionProvider)
        {
            _cache = new TypeLightDictionary<ThreadExecutionModeDictionary>(3);
            _threadDispatcher = threadDispatcher;
            _metadataContextProvider = metadataContextProvider;
            _subscribers = new HashSet<MessengerSubscriberInfo>(this);
            _contextProviders = Default.EmptyArray<IMessageContextProviderComponent>();
            _handlerComponents = Default.EmptyArray<IMessengerHandlerComponent>();
            _listeners = Default.EmptyArray<IMessengerHandlerListener>();
        }

        #endregion

        #region Implementation of interfaces

        void IComponentOwnerAddedCallback<IComponent<IMessenger>>.OnComponentAdded(IComponentCollection<IComponent<IMessenger>> collection, IComponent<IMessenger> component,
            IReadOnlyMetadataContext? metadata)
        {
            MugenExtensions.ComponentTrackerOnAdded(ref _contextProviders, this, collection, component, metadata);
            MugenExtensions.ComponentTrackerOnAdded(ref _handlerComponents, this, collection, component, metadata);
            MugenExtensions.ComponentTrackerOnAdded(ref _listeners, this, collection, component, metadata);
        }

        void IComponentOwnerRemovedCallback<IComponent<IMessenger>>.OnComponentRemoved(IComponentCollection<IComponent<IMessenger>> collection, IComponent<IMessenger> component,
            IReadOnlyMetadataContext? metadata)
        {
            MugenExtensions.ComponentTrackerOnRemoved(ref _contextProviders, collection, component, metadata);
            MugenExtensions.ComponentTrackerOnRemoved(ref _handlerComponents, collection, component, metadata);
            MugenExtensions.ComponentTrackerOnRemoved(ref _listeners, collection, component, metadata);
        }

        bool IEqualityComparer<MessengerSubscriberInfo>.Equals(MessengerSubscriberInfo x, MessengerSubscriberInfo y)
        {
            return x.Subscriber.Equals(y.Subscriber);
        }

        int IEqualityComparer<MessengerSubscriberInfo>.GetHashCode(MessengerSubscriberInfo obj)
        {
            return obj.Subscriber.GetHashCode();
        }

        public void Dispose()
        {
            this.UnsubscribeAll();
            this.ClearComponents();
        }

        public bool Subscribe(object subscriber, ThreadExecutionMode? executionMode = null, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(subscriber, nameof(subscriber));
            if (executionMode == null)
                executionMode = ThreadExecutionMode.Current;

            var components = GetComponents();
            var added = false;
            for (var i = 0; i < components.Length; i++)
            {
                var handler = (components[i] as IMessengerSubscriberComponent)?.TryGetSubscriber(subscriber, executionMode, metadata)!;
                if (handler == null)
                    continue;

                if (AddSubscriber(handler, executionMode, metadata))
                    added = true;
            }

            return added;
        }

        public bool Unsubscribe(object subscriber, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(subscriber, nameof(subscriber));
            var removed = false;
            lock (_subscribers)
            {
                var info = new MessengerSubscriberInfo(subscriber, ThreadExecutionMode.Current);
                while (_subscribers.Remove(info))
                    removed = true;

                if (removed)
                    _cache.Clear();
            }

            if (removed)
            {
                var components = GetComponents();
                for (var i = 0; i < components.Length; i++)
                    (components[i] as IMessengerSubscriberListener)?.OnUnsubscribed(this, subscriber, metadata);
            }

            return removed;
        }

        public IReadOnlyList<MessengerSubscriberInfo> GetSubscribers()
        {
            lock (_subscribers)
            {
                return _subscribers.ToArray();
            }
        }

        public IMessageContext Publish(object sender, object message, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(message, nameof(message));
            IMessageContext? ctx = null;
            for (var i = 0; i < _contextProviders.Length; i++)
            {
                ctx = _contextProviders[i].TryGetMessengerContext(sender, message, metadata);
                if (ctx != null)
                    break;
            }

            if (ctx == null)
                ctx = new MessageContext(this, sender, message, metadata);

            PublishInternal(ctx);
            return ctx;
        }

        public void Publish(IMessageContext messageContext)
        {
            Should.NotBeNull(messageContext, nameof(messageContext));
            PublishInternal(messageContext);
        }

        #endregion

        #region Methods

        private void PublishInternal(IMessageContext messageContext)
        {
            var threadDispatcher = _threadDispatcher.ServiceIfNull();
            ThreadExecutionModeDictionary? dictionary;
            lock (_subscribers)
            {
                var key = messageContext.Message.GetType();
                if (!_cache.TryGetValue(key, out dictionary))
                {
                    foreach (var subscriber in _subscribers)
                    {
                        var canHandle = false;
                        for (var i = 0; i < _handlerComponents.Length; i++)
                        {
                            if (_handlerComponents[i].CanHandle(subscriber.Subscriber, messageContext))
                            {
                                canHandle = true;
                                break;
                            }
                        }

                        if (!canHandle)
                            continue;

                        if (dictionary == null)
                            dictionary = new ThreadExecutionModeDictionary();

                        if (!dictionary.TryGetValue(subscriber.ExecutionMode, out var value))
                        {
                            value = new MessageThreadExecutor(this);
                            dictionary[subscriber.ExecutionMode] = value;
                        }

                        value.Add(subscriber.Subscriber);
                    }

                    _cache[key] = dictionary;
                }
            }

            if (dictionary != null)
            {
                foreach (var dispatcherExecutor in dictionary)
                    threadDispatcher.Execute(dispatcherExecutor.Key, dispatcherExecutor.Value, messageContext);
            }
        }

        private bool AddSubscriber(object subscriber, ThreadExecutionMode executionMode, IReadOnlyMetadataContext? metadata)
        {
            bool added;
            lock (_subscribers)
            {
                added = _subscribers.Add(new MessengerSubscriberInfo(subscriber, executionMode));
                if (added)
                    _cache.Clear();
            }

            var components = GetComponents();
            if (added)
            {
                for (var i = 0; i < components.Length; i++)
                    (components[i] as IMessengerSubscriberListener)?.OnSubscribed(this, subscriber, executionMode, metadata);
            }

            return added;
        }

        #endregion

        #region Nested types

        private sealed class ThreadExecutionModeDictionary : LightDictionary<ThreadExecutionMode, MessageThreadExecutor>
        {
            #region Constructors

            public ThreadExecutionModeDictionary() : base(3)
            {
            }

            #endregion

            #region Methods

            protected override bool Equals(ThreadExecutionMode x, ThreadExecutionMode y)
            {
                return x == y;
            }

            protected override int GetHashCode(ThreadExecutionMode key)
            {
                return key.GetHashCode();
            }

            #endregion
        }

        private sealed class MessageThreadExecutor : List<object>, IHasStateThreadDispatcherHandler
        {
            #region Fields

            private readonly Messenger _messenger;

            #endregion

            #region Constructors

            public MessageThreadExecutor(Messenger messenger)
            {
                _messenger = messenger;
            }

            #endregion

            #region Properties

            object IHasStateThreadDispatcherHandler.State { get; set; }

            #endregion

            #region Implementation of interfaces

            public void Execute(object? state)
            {
                var messageContext = (IMessageContext) state!;
                var handlers = _messenger._handlerComponents;
                var listeners = _messenger._listeners;
                for (var i = 0; i < Count; i++)
                {
                    var subscriber = this[i];
                    for (var j = 0; j < listeners.Length; j++)
                        listeners[j].OnHandling(subscriber, messageContext);

                    MessengerResult? result = null;
                    for (var j = 0; j < handlers.Length; j++)
                    {
                        result = handlers[j].TryHandle(subscriber, messageContext);
                        if (result != null)
                            break;
                    }

                    for (var j = 0; j < listeners.Length; j++)
                        listeners[j].OnHandled(result, subscriber, messageContext);

                    if (result == MessengerResult.Invalid)
                        _messenger.Unsubscribe(subscriber, messageContext.Metadata);
                }
            }

            #endregion
        }

        private sealed class MessageContext : IMessageContext
        {
            #region Fields

            private readonly Messenger _messenger;
            private IReadOnlyMetadataContext? _metadata;

            #endregion

            #region Constructors

            public MessageContext(Messenger messenger, object? sender, object message, IReadOnlyMetadataContext? metadata)
            {
                _metadata = metadata;
                _messenger = messenger;
                Sender = sender;
                Message = message;
            }

            #endregion

            #region Properties

            public bool HasMetadata => _metadata != null;

            public IMetadataContext Metadata
            {
                get
                {
                    if (_metadata is IMetadataContext ctx)
                        return ctx;

                    Interlocked.CompareExchange(ref _metadata, _metadata.ToNonReadonly(this, _messenger._metadataContextProvider), null);
                    return (IMetadataContext) _metadata!;
                }
            }

            public object? Sender { get; }

            public object Message { get; }

            #endregion
        }

        #endregion
    }
}