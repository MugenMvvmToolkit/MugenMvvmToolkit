using System;
using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Collections;
using MugenMvvm.Collections.Internal;
using MugenMvvm.Components;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Messaging;
using MugenMvvm.Interfaces.Messaging.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Threading;

namespace MugenMvvm.Messaging
{
    public sealed class Messenger : ComponentOwnerBase<IMessenger>, IMessenger, IHasAddedCallbackComponentOwner, IHasRemovedCallbackComponentOwner, IEqualityComparer<MessengerSubscriberInfo>//todo review subscribe/unsubscribe equality 
    {
        #region Fields

        private readonly TypeLightDictionary<ThreadExecutionModeDictionary?> _cache;
        private readonly IMetadataContextProvider? _metadataContextProvider;
        private readonly HashSet<MessengerSubscriberInfo> _subscribers;
        private readonly IThreadDispatcher? _threadDispatcher;

        #endregion

        #region Constructors

        public Messenger(IThreadDispatcher? threadDispatcher = null, IComponentCollectionProvider? componentCollectionProvider = null,
            IMetadataContextProvider? metadataContextProvider = null)
            : base(componentCollectionProvider)
        {
            _cache = new TypeLightDictionary<ThreadExecutionModeDictionary?>(3);
            _threadDispatcher = threadDispatcher;
            _metadataContextProvider = metadataContextProvider;
            _subscribers = new HashSet<MessengerSubscriberInfo>(this);
        }

        #endregion

        #region Implementation of interfaces

        bool IEqualityComparer<MessengerSubscriberInfo>.Equals(MessengerSubscriberInfo x, MessengerSubscriberInfo y)
        {
            return x.Subscriber.Equals(y.Subscriber);
        }

        int IEqualityComparer<MessengerSubscriberInfo>.GetHashCode(MessengerSubscriberInfo obj)
        {
            return obj.Subscriber.GetHashCode();
        }

        void IHasAddedCallbackComponentOwner.OnComponentAdded(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata)
        {
            if (!(component is IMessengerHandlerComponent))
                return;
            lock (_subscribers)
            {
                _cache.Clear();
            }
        }

        void IHasRemovedCallbackComponentOwner.OnComponentRemoved(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata)
        {
            if (!(component is IMessengerHandlerComponent))
                return;
            lock (_subscribers)
            {
                _cache.Clear();
            }
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

            var components = GetComponents<IMessengerSubscriberComponent>(metadata);
            var added = false;
            for (var i = 0; i < components.Length; i++)
            {
                var handler = components[i].TryGetSubscriber(subscriber, executionMode, metadata);
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
                GetComponents<IMessengerSubscriberListener>(metadata).OnUnsubscribed(this, subscriber, metadata);
            return removed;
        }

        public IReadOnlyList<MessengerSubscriberInfo> GetSubscribers()
        {
            lock (_subscribers)
            {
                return _subscribers.ToArray();
            }
        }

        public IMessageContext Publish(object? sender, object message, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(message, nameof(message));
            var ctx = GetComponents<IMessageContextProviderComponent>(metadata).TryGetMessengerContext(sender, message, metadata) ?? new MessageContext(this, sender, message, metadata);
            Publish(ctx);
            return ctx;
        }

        public void Publish(IMessageContext messageContext)
        {
            Should.NotBeNull(messageContext, nameof(messageContext));
            var threadDispatcher = _threadDispatcher.DefaultIfNull();
            ThreadExecutionModeDictionary? dictionary;
            lock (_subscribers)
            {
                var key = messageContext.Message.GetType();
                if (!_cache.TryGetValue(key, out dictionary))
                {
                    dictionary = GetHandlers(key);
                    _cache[key] = dictionary;
                }
            }

            if (dictionary != null)
            {
                foreach (var dispatcherExecutor in dictionary)
                    threadDispatcher.Execute(dispatcherExecutor.Key, dispatcherExecutor.Value, messageContext);
            }
        }

        #endregion

        #region Methods

        private ThreadExecutionModeDictionary? GetHandlers(Type messageType)
        {
            var handlerComponents = GetComponents<IMessengerHandlerComponent>();
            ThreadExecutionModeDictionary? dictionary = null;
            foreach (var subscriber in _subscribers)
            {
                if (!handlerComponents.CanHandle(subscriber.Subscriber, messageType))
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

            return dictionary;
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

            if (added)
                GetComponents<IMessengerSubscriberListener>(metadata).OnSubscribed(this, subscriber, executionMode, metadata);
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

        private sealed class MessageThreadExecutor : List<object>, IThreadDispatcherHandler<IMessageContext>, IValueHolder<Delegate>
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

            Delegate? IValueHolder<Delegate>.Value { get; set; }

            #endregion

            #region Implementation of interfaces

            public void Execute(IMessageContext messageContext)
            {
                var metadata = messageContext.GetMetadataOrDefault();
                var handlers = _messenger.GetComponents<IMessengerHandlerComponent>(metadata);
                var listeners = _messenger.GetComponents<IMessengerHandlerListener>(metadata);
                for (var i = 0; i < Count; i++)
                {
                    var subscriber = this[i];
                    listeners.OnHandling(_messenger, subscriber, messageContext);
                    var result = handlers.TryHandle(subscriber, messageContext);
                    listeners.OnHandled(_messenger, result, subscriber, messageContext);

                    if (result == MessengerResult.Invalid)
                        _messenger.Unsubscribe(subscriber, messageContext.Metadata);
                }
            }

            #endregion
        }

        public sealed class MessageContext : IMessageContext
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

            public bool HasMetadata => !_metadata.IsNullOrEmpty();

            public IMetadataContext Metadata
            {
                get
                {
                    if (_metadata is IMetadataContext ctx)
                        return ctx;
                    return _messenger._metadataContextProvider.LazyInitializeNonReadonly(ref _metadata, this);
                }
            }

            public object? Sender { get; }

            public object Message { get; }

            #endregion
        }

        #endregion
    }
}