using System.Collections.Generic;
using System.Linq;
using System.Threading;
using MugenMvvm.Attributes;
using MugenMvvm.Collections;
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

        private readonly IMetadataContextProvider? _metadataContextProvider;

        private readonly HashSet<MessengerSubscriberInfo> _subscribers;
        private readonly IThreadDispatcher? _threadDispatcher;

        private IMessageContextProviderComponent[] _contextProviders;
        private IMessengerSubscriberDecoratorComponent[] _decorators;
        private IMessengerHandlerComponent[] _handlers;
        private IMessengerListener[] _listeners;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public Messenger(IThreadDispatcher? threadDispatcher = null, IComponentCollectionProvider? componentCollectionProvider = null,
            IMetadataContextProvider? metadataContextProvider = null)
            : base(componentCollectionProvider)
        {
            _threadDispatcher = threadDispatcher;
            _metadataContextProvider = metadataContextProvider;
            _subscribers = new HashSet<MessengerSubscriberInfo>(this);
            _contextProviders = Default.EmptyArray<IMessageContextProviderComponent>();
            _decorators = Default.EmptyArray<IMessengerSubscriberDecoratorComponent>();
            _handlers = Default.EmptyArray<IMessengerHandlerComponent>();
            _listeners = Default.EmptyArray<IMessengerListener>();
        }

        #endregion

        #region Implementation of interfaces

        void IComponentOwnerAddedCallback<IComponent<IMessenger>>.OnComponentAdded(IComponentCollection<IComponent<IMessenger>> collection, IComponent<IMessenger> component,
            IReadOnlyMetadataContext? metadata)
        {
            MugenExtensions.ComponentTrackerOnAdded(ref _contextProviders, this, collection, component, metadata);
            MugenExtensions.ComponentTrackerOnAdded(ref _decorators, this, collection, component, metadata);
            MugenExtensions.ComponentTrackerOnAdded(ref _handlers, this, collection, component, metadata);
            MugenExtensions.ComponentTrackerOnAdded(ref _listeners, this, collection, component, metadata);
        }

        void IComponentOwnerRemovedCallback<IComponent<IMessenger>>.OnComponentRemoved(IComponentCollection<IComponent<IMessenger>> collection, IComponent<IMessenger> component,
            IReadOnlyMetadataContext? metadata)
        {
            MugenExtensions.ComponentTrackerOnRemoved(ref _contextProviders, collection, component, metadata);
            MugenExtensions.ComponentTrackerOnRemoved(ref _decorators, collection, component, metadata);
            MugenExtensions.ComponentTrackerOnRemoved(ref _handlers, collection, component, metadata);
            MugenExtensions.ComponentTrackerOnRemoved(ref _listeners, collection, component, metadata);
        }

        public bool Equals(MessengerSubscriberInfo x, MessengerSubscriberInfo y)
        {
            return x.Subscriber.Equals(y.Subscriber);
        }

        public int GetHashCode(MessengerSubscriberInfo obj)
        {
            return obj.Subscriber.GetHashCode();
        }

        public IMessageContext Publish(object? sender, object message, IReadOnlyMetadataContext? metadata = null)
        {
            var context = GetMessageContext(sender, message, metadata);
            PublishInternal(context, true);
            return context;
        }

        public void Publish(IMessageContext messageContext)
        {
            Should.NotBeNull(messageContext, nameof(messageContext));
            PublishInternal(messageContext, false);
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
            for (var i = 0; i < _decorators.Length; i++)
            {
                subscriber = _decorators[i].OnSubscribing(subscriber, executionMode, metadata)!;
                if (subscriber == null)
                    return false;
            }

            bool added;
            lock (_subscribers)
            {
                added = _subscribers.Add(new MessengerSubscriberInfo(subscriber, executionMode));
            }

            if (added)
            {
                for (var i = 0; i < _listeners.Length; i++)
                    _listeners[i].OnSubscribed(this, subscriber, executionMode, metadata);
            }

            return true;
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
            }

            if (removed)
            {
                for (var i = 0; i < _listeners.Length; i++)
                    _listeners[i].OnUnsubscribed(this, subscriber, metadata);
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

        #endregion

        #region Methods

        private IMessageContext GetMessageContext(object? sender, object message, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(message, nameof(message));
            IMessageContext? ctx = null;
            for (var i = 0; i < _contextProviders.Length; i++)
            {
                ctx = _contextProviders[i].TryGetMessengerContext(metadata);
                if (ctx != null)
                    break;
            }

            if (ctx == null)
                ctx = new MessageContext(this, sender, message, metadata);

            for (var i = 0; i < _listeners.Length; i++)
                _listeners[i].OnContextCreated(this, message, ctx);
            return ctx!;
        }

        private void PublishInternal(IMessageContext messageContext, bool isOwner)
        {
            var threadDispatcher = _threadDispatcher.ServiceIfNull();
            ThreadExecutionModeDictionary? dictionary = null;
            MessageThreadExecutor? inlineExecutor = null;
            var localContext = isOwner ? messageContext as MessageContext : null;
            lock (_subscribers)
            {
                foreach (var subscriber in _subscribers)
                {
                    if (localContext == null)
                    {
                        if (!messageContext.MarkAsHandled(subscriber.Subscriber))
                            continue;
                    }
                    else
                    {
                        if (!localContext.MarkAsHandledNoLock(subscriber.Subscriber))
                            continue;
                    }


                    if (threadDispatcher.CanExecuteInline(subscriber.ExecutionMode))
                    {
                        if (inlineExecutor == null)
                            inlineExecutor = new MessageThreadExecutor(this, messageContext);
                        inlineExecutor.Add(subscriber.Subscriber);
                    }
                    else
                    {
                        if (dictionary == null)
                            dictionary = new ThreadExecutionModeDictionary();

                        if (!dictionary.TryGetValue(subscriber.ExecutionMode, out var value))
                        {
                            value = new MessageThreadExecutor(this, messageContext);
                            dictionary[subscriber.ExecutionMode] = value;
                        }

                        value.Add(subscriber.Subscriber);
                    }
                }
            }

            inlineExecutor?.Execute(null);
            if (dictionary != null)
            {
                foreach (var dispatcherExecutor in dictionary)
                    threadDispatcher.Execute(dispatcherExecutor.Value, dispatcherExecutor.Key, null);
            }
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
                return x.Equals(y);
            }

            protected override int GetHashCode(ThreadExecutionMode key)
            {
                return key.GetHashCode();
            }

            #endregion
        }

        private sealed class MessageThreadExecutor : List<object>, IThreadDispatcherHandler
        {
            #region Fields

            private readonly IMessageContext _messageContext;
            private readonly Messenger _messenger;

            #endregion

            #region Constructors

            public MessageThreadExecutor(Messenger messenger, IMessageContext messageContext)
            {
                _messenger = messenger;
                _messageContext = messageContext;
            }

            #endregion

            #region Implementation of interfaces

            public void Execute(object? state)
            {
                var handlers = _messenger._handlers;
                var listeners = _messenger._listeners;
                for (var i = 0; i < Count; i++)
                {
                    var subscriber = this[i];
                    for (var j = 0; j < listeners.Length; i++)
                        listeners[j].OnHandling(subscriber, _messageContext);

                    MessengerResult? result = null;
                    for (var j = 0; j < handlers.Length; j++)
                    {
                        result = handlers[j].TryHandle(subscriber, _messageContext);
                        if (result != null)
                            break;
                    }

                    for (var j = 0; j < listeners.Length; i++)
                        listeners[j].OnHandled(result, subscriber, _messageContext);

                    if (result == MessengerResult.Invalid)
                        _messenger.Unsubscribe(subscriber, _messageContext.Metadata);
                }
            }

            #endregion
        }

        private sealed class MessageContext : HashSet<object>, IMessageContext
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

            public IMessagePublisher Publisher => _messenger;

            public object? Sender { get; }

            public object Message { get; }

            #endregion

            #region Implementation of interfaces

            public bool MarkAsHandled(object subscriber)
            {
                lock (this)
                {
                    return Add(subscriber);
                }
            }

            #endregion

            #region Methods

            public bool MarkAsHandledNoLock(object subscriber)
            {
                return Add(subscriber);
            }

            #endregion
        }

        #endregion
    }
}