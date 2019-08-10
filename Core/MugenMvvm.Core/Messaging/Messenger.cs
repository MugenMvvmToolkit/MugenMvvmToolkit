using System.Collections.Generic;
using System.Threading.Tasks;
using MugenMvvm.Attributes;
using MugenMvvm.Components;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Messaging;
using MugenMvvm.Interfaces.Messaging.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Threading;

namespace MugenMvvm.Messaging
{
    public sealed class Messenger : ComponentOwnerBase<IMessenger>, IMessenger, IEqualityComparer<KeyValuePair<ThreadExecutionMode, IMessengerSubscriber>>,
        IComponentOwnerAddedCallback<IComponent<IMessenger>>, IComponentOwnerRemovedCallback<IComponent<IMessenger>>
    {
        #region Fields

        private readonly HashSet<KeyValuePair<ThreadExecutionMode, IMessengerSubscriber>> _subscribers;
        private readonly IMetadataContextProvider? _metadataContextProvider;
        private readonly IThreadDispatcher? _threadDispatcher;

        private bool _hasFactoryComponents;
        private bool _hasListenerComponents;
        private bool _hasMessageInterceptorComponents;
        private bool _hasMessageSubscriberDecoratorComponents;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public Messenger(IThreadDispatcher? threadDispatcher = null, IComponentCollectionProvider? componentCollectionProvider = null, IMetadataContextProvider? metadataContextProvider = null)
            : base(componentCollectionProvider)
        {
            _threadDispatcher = threadDispatcher;
            _metadataContextProvider = metadataContextProvider;
            _subscribers = new HashSet<KeyValuePair<ThreadExecutionMode, IMessengerSubscriber>>(this);
        }

        #endregion

        #region Implementation of interfaces

        void IComponentOwnerAddedCallback<IComponent<IMessenger>>.OnComponentAdded(object collection, IComponent<IMessenger> component, IReadOnlyMetadataContext? metadata)
        {
            OnComponentsChanged();
        }

        void IComponentOwnerRemovedCallback<IComponent<IMessenger>>.OnComponentRemoved(object collection, IComponent<IMessenger> component, IReadOnlyMetadataContext? metadata)
        {
            OnComponentsChanged();
        }

        bool IEqualityComparer<KeyValuePair<ThreadExecutionMode, IMessengerSubscriber>>.Equals(KeyValuePair<ThreadExecutionMode, IMessengerSubscriber> x, KeyValuePair<ThreadExecutionMode, IMessengerSubscriber> y)
        {
            return x.Value.Equals(y.Value);
        }

        int IEqualityComparer<KeyValuePair<ThreadExecutionMode, IMessengerSubscriber>>.GetHashCode(KeyValuePair<ThreadExecutionMode, IMessengerSubscriber> obj)
        {
            return obj.Value.GetHashCode();
        }

        public IMessengerContext GetMessengerContext(IMetadataContext? metadata = null)
        {
            return GetMessengerContextInternal(metadata);
        }

        public void Publish(object sender, object message, IMessengerContext? messengerContext = null)
        {
            PublishInternalAsync(sender, message, messengerContext, false);
        }

        public Task PublishAsync(object sender, object message, IMessengerContext? messengerContext = null)
        {
            return PublishInternalAsync(sender, message, messengerContext, true);
        }

        public void Subscribe(IMessengerSubscriber subscriber, ThreadExecutionMode executionMode, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(subscriber, nameof(subscriber));
            Should.NotBeNull(executionMode, nameof(executionMode));
            if (_hasMessageSubscriberDecoratorComponents)
            {
                var components = GetComponents();
                for (var i = 0; i < components.Length; i++)
                {
                    if (components[i] is ISubscriberDecoratorComponent decorator)
                        subscriber = decorator.OnSubscribing(subscriber, executionMode, metadata);
                }
            }

            bool added;
            lock (_subscribers)
            {
                added = _subscribers.Add(new KeyValuePair<ThreadExecutionMode, IMessengerSubscriber>(executionMode, subscriber));
            }

            if (added && _hasListenerComponents)
            {
                var components = GetComponents();
                for (var i = 0; i < components.Length; i++)
                    (components[i] as IMessengerListener)?.OnSubscribed(this, subscriber, executionMode, metadata);
            }
        }

        public bool Unsubscribe(IMessengerSubscriber subscriber, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(subscriber, nameof(subscriber));
            bool removed;
            lock (_subscribers)
            {
                removed = _subscribers.Remove(new KeyValuePair<ThreadExecutionMode, IMessengerSubscriber>(ThreadExecutionMode.Current, subscriber));
            }

            if (removed && _hasListenerComponents)
            {
                var components = GetComponents();
                for (var i = 0; i < components.Length; i++)
                    (components[i] as IMessengerListener)?.OnUnsubscribed(this, subscriber, metadata);
            }

            return removed;
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

        public void Dispose()
        {
            this.UnsubscribeAll();
            this.ClearComponents();
        }

        #endregion

        #region Methods

        private void OnComponentsChanged()
        {
            var components = GetComponents();
            _hasListenerComponents = false;
            _hasFactoryComponents = false;
            _hasMessageInterceptorComponents = false;
            _hasMessageSubscriberDecoratorComponents = false;
            for (var i = 0; i < components.Length; i++)
            {
                var component = components[i];
                if (component is IMessengerListener)
                    _hasListenerComponents = true;
                else if (component is IMessengerContextProviderComponent)
                    _hasFactoryComponents = true;
                else if (component is IMessageInterceptorComponent)
                    _hasMessageInterceptorComponents = true;
                else if (component is ISubscriberDecoratorComponent)
                    _hasMessageSubscriberDecoratorComponents = true;

                if (_hasListenerComponents && _hasFactoryComponents && _hasMessageInterceptorComponents && _hasMessageSubscriberDecoratorComponents)
                    return;
            }
        }

        private IMessengerContext GetMessengerContextInternal(IMetadataContext? metadata)
        {
            IMessengerContext? ctx = null;
            if (_hasFactoryComponents)
            {
                var components = GetComponents();
                for (var i = 0; i < components.Length; i++)
                {
                    if (components[i] is IMessengerContextProviderComponent factory)
                    {
                        ctx = factory.TryGetMessengerContext(metadata);
                        if (ctx != null)
                            break;
                    }
                }
            }

            if (ctx == null)
                ctx = new MessengerContext(this, metadata);

            if (_hasListenerComponents)
            {
                var components = GetComponents();
                for (var i = 0; i < components.Length; i++)
                    (components[i] as IMessengerListener)?.OnContextCreated(this, ctx);
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
                messengerContext = GetMessengerContext(null);
                rawContext = messengerContext as MessengerContext;
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
                var index = 0;
                foreach (var dispatcherExecutor in dictionary)
                {
                    tasks[index] = _threadDispatcher.ServiceIfNull().ExecuteAsync(dispatcherExecutor.Value, dispatcherExecutor.Key, null);
                    ++index;
                }

                return Task.WhenAll(tasks);
            }

            foreach (var dispatcherExecutor in dictionary)
                _threadDispatcher.ServiceIfNull().Execute(dispatcherExecutor.Value, dispatcherExecutor.Key, null);
            return Default.CompletedTask;
        }

        #endregion

        #region Nested types

        private sealed class ThreadDispatcherExecutor : List<IMessengerSubscriber>, IThreadDispatcherHandler
        {
            #region Fields

            private readonly object _message;
            private readonly Messenger _messenger;

            private readonly IMessengerContext _messengerContext;
            private readonly object _sender;

            #endregion

            #region Constructors

            public ThreadDispatcherExecutor(Messenger messenger, object sender, object message, IMessengerContext messengerContext)
            {
                _messenger = messenger;
                _sender = sender;
                _message = message;
                _messengerContext = messengerContext;
            }

            #endregion

            #region Implementation of interfaces

            public void Execute(object? state)
            {
                if (_messenger._hasMessageInterceptorComponents)
                {
                    for (var i = 0; i < Count; i++)
                        PublishAndNotify(this[i]);
                }
                else
                {
                    for (var i = 0; i < Count; i++)
                    {
                        if (this[i].Handle(_sender, _message, _messengerContext) == MessengerSubscriberResult.Invalid)
                            _messenger.Unsubscribe(this[i]);
                    }
                }
            }

            #endregion

            #region Methods

            private void PublishAndNotify(IMessengerSubscriber subscriber)
            {
                var components = _messenger.GetComponents();
                MessengerSubscriberResult? subscriberResult = null;
                for (var i = 0; i < components.Length; i++)
                {
                    subscriberResult = (components[i] as IMessageInterceptorComponent)?.OnPublishing(subscriber, _sender, _message, _messengerContext);
                    if (subscriberResult != null)
                        break;
                }

                var result = subscriberResult ?? subscriber.Handle(_sender, _message, _messengerContext);

                for (var i = 0; i < components.Length; i++)
                    (components[i] as IMessageInterceptorComponent)?.OnPublished(result, subscriber, _sender, _message, _messengerContext);

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

            public bool HasMetadata => _metadata != null;

            public IMetadataContext Metadata
            {
                get
                {
                    if (_metadata == null)
                        ((Messenger)Messenger)._metadataContextProvider.LazyInitialize(ref _metadata, this);
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