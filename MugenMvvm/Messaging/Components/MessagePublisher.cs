using System;
using System.Collections.Generic;
using MugenMvvm.Attributes;
using MugenMvvm.Constants;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Messaging;
using MugenMvvm.Interfaces.Messaging.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Threading;
using MugenMvvm.Internal;

namespace MugenMvvm.Messaging.Components
{
    public sealed class MessagePublisher : IMessagePublisherComponent, IHasPriority, IAttachableComponent, IDetachableComponent, IComponentCollectionChangedListener, IHasCache
    {
        private readonly Dictionary<Type, Dictionary<ThreadExecutionMode, MessageThreadExecutor>?> _cache;
        private readonly IThreadDispatcher? _threadDispatcher;
        private IMessenger? _owner;

        [Preserve(Conditional = true)]
        public MessagePublisher(IThreadDispatcher? threadDispatcher = null)
        {
            _threadDispatcher = threadDispatcher;
            DefaultExecutionMode = ThreadExecutionMode.Current;
            _cache = new Dictionary<Type, Dictionary<ThreadExecutionMode, MessageThreadExecutor>?>(InternalEqualityComparer.Type);
        }

        public ThreadExecutionMode DefaultExecutionMode { get; set; }

        public int Priority { get; set; } = MessengerComponentPriority.Publisher;

        private static Dictionary<ThreadExecutionMode, MessageThreadExecutor>? GetHandlers(IMessenger messenger, Type messageType, ThreadExecutionMode defaultMode,
            IReadOnlyMetadataContext? metadata)
        {
            var handlers = messenger
                           .GetComponents<IMessengerSubscriberComponent>(metadata)
                           .TryGetMessengerHandlers(messenger, messageType, metadata);

            if (handlers.Count == 0)
                return null;

            var dictionary = new Dictionary<ThreadExecutionMode, MessageThreadExecutor>(InternalEqualityComparer.ThreadExecutionMode);
            foreach (var subscriber in handlers)
            {
                var mode = subscriber.ExecutionMode ?? defaultMode;
                if (!dictionary.TryGetValue(mode, out var value))
                {
                    value = new MessageThreadExecutor(messenger);
                    dictionary[mode] = value;
                }

                value.Add(subscriber);
            }

            return dictionary;
        }

        public void Invalidate(object? state = null, IReadOnlyMetadataContext? metadata = null)
        {
            lock (_cache)
            {
                _cache.Clear();
            }
        }

        public bool TryPublish(IMessenger messenger, IMessageContext messageContext)
        {
            var threadDispatcher = _threadDispatcher.DefaultIfNull();
            Dictionary<ThreadExecutionMode, MessageThreadExecutor>? dictionary;
            lock (_cache)
            {
                var key = messageContext.Message.GetType();
                if (!_cache.TryGetValue(key, out dictionary))
                {
                    dictionary = GetHandlers(messenger, key, DefaultExecutionMode, messageContext.GetMetadataOrDefault());
                    _cache[key] = dictionary;
                }
            }

            if (dictionary == null)
                return false;
            foreach (var dispatcherExecutor in dictionary)
                threadDispatcher.Execute(dispatcherExecutor.Key, dispatcherExecutor.Value, messageContext);
            return true;
        }

        bool IAttachableComponent.OnAttaching(object owner, IReadOnlyMetadataContext? metadata) => true;

        void IAttachableComponent.OnAttached(object owner, IReadOnlyMetadataContext? metadata)
        {
            if (owner is not IMessenger messenger)
                return;
            if (_owner != null)
                ExceptionManager.ThrowObjectInitialized(this);
            _owner = messenger;
            if (_owner != null)
            {
                _owner.Components.AddComponent(this, metadata);
                Invalidate(null, metadata);
            }
        }

        void IComponentCollectionChangedListener.OnAdded(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata)
        {
            if (component is IMessengerSubscriberComponent)
                Invalidate(null, metadata);
        }

        void IComponentCollectionChangedListener.OnRemoved(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata)
        {
            if (component is IMessengerSubscriberComponent)
                Invalidate(null, metadata);
        }

        bool IDetachableComponent.OnDetaching(object owner, IReadOnlyMetadataContext? metadata) => true;

        void IDetachableComponent.OnDetached(object owner, IReadOnlyMetadataContext? metadata)
        {
            if (owner == _owner)
            {
                _owner?.Components.RemoveComponent(this, metadata);
                _owner = null;
                Invalidate(null, metadata);
            }
        }

        private sealed class MessageThreadExecutor : List<MessengerHandler>, IThreadDispatcherHandler, IValueHolder<Delegate>
        {
            private readonly IMessenger _messenger;

            public MessageThreadExecutor(IMessenger messenger)
            {
                _messenger = messenger;
            }

            Delegate? IValueHolder<Delegate>.Value { get; set; }

            public void Execute(object? state)
            {
                var messageContext = (IMessageContext) state!;
                for (var i = 0; i < Count; i++)
                {
                    if (this[i].Handle(messageContext) == MessengerResult.Invalid)
                        _messenger.TryUnsubscribe(this[i].Subscriber!, messageContext.GetMetadataOrDefault());
                }
            }
        }
    }
}