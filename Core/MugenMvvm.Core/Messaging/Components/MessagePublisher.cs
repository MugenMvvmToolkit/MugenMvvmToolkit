using System;
using System.Collections.Generic;
using MugenMvvm.Collections;
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

namespace MugenMvvm.Messaging.Components
{
    public sealed class MessagePublisher : LightDictionary<Type, MessagePublisher.ThreadExecutionModeDictionary?>, IMessagePublisherComponent, IHasPriority,
        IAttachableComponent, IDetachableComponent, IHasCache
    {
        #region Fields

        private readonly IThreadDispatcher? _threadDispatcher;
        private IMessenger? _owner;

        #endregion

        #region Constructors

        public MessagePublisher(IThreadDispatcher? threadDispatcher = null) : base(3)
        {
            _threadDispatcher = threadDispatcher;
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = MessengerComponentPriority.Publisher;

        #endregion

        #region Implementation of interfaces

        bool IAttachableComponent.OnAttaching(object owner, IReadOnlyMetadataContext? metadata)
        {
            return true;
        }

        void IAttachableComponent.OnAttached(object owner, IReadOnlyMetadataContext? metadata)
        {
            _owner = owner as IMessenger;
            if (_owner != null)
                Invalidate<object?>(null, metadata);
        }

        bool IDetachableComponent.OnDetaching(object owner, IReadOnlyMetadataContext? metadata)
        {
            return true;
        }

        void IDetachableComponent.OnDetached(object owner, IReadOnlyMetadataContext? metadata)
        {
            if (ReferenceEquals(owner, _owner))
            {
                _owner = null;
                Invalidate<object?>(null, metadata);
            }
        }

        public void Invalidate<TState>(in TState state, IReadOnlyMetadataContext? metadata)
        {
            lock (this)
            {
                Clear();
            }
        }

        public void TryPublish(IMessageContext messageContext)
        {
            Should.NotBeNull(messageContext, nameof(messageContext));
            var messenger = _owner;
            if (messenger == null)
                return;

            var threadDispatcher = _threadDispatcher.DefaultIfNull();
            ThreadExecutionModeDictionary? dictionary;
            lock (this)
            {
                var key = messageContext.Message.GetType();
                if (!TryGetValue(key, out dictionary))
                {
                    dictionary = GetHandlers(messenger, key, messageContext.GetMetadataOrDefault());
                    this[key] = dictionary;
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

        private static ThreadExecutionModeDictionary? GetHandlers(IMessenger messenger, Type messageType, IReadOnlyMetadataContext? metadata)
        {
            var handlers = messenger.GetComponents<IMessengerSubscriberComponent>().TryGetMessengerHandlers(messageType, metadata);
            if (handlers == null || handlers.Count == 0)
                return null;

            var dictionary = new ThreadExecutionModeDictionary();
            for (var index = 0; index < handlers.Count; index++)
            {
                var subscriber = handlers[index];
                if (!dictionary.TryGetValue(subscriber.ExecutionMode, out var value))
                {
                    value = new MessageThreadExecutor(messenger);
                    dictionary[subscriber.ExecutionMode] = value;
                }

                value.Add(subscriber);
            }

            return dictionary;
        }

        protected override bool Equals(Type x, Type y)
        {
            return x == y;
        }

        protected override int GetHashCode(Type key)
        {
            return key.GetHashCode();
        }

        #endregion

        #region Nested types

        public sealed class ThreadExecutionModeDictionary : LightDictionary<ThreadExecutionMode, MessageThreadExecutor>
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

        public sealed class MessageThreadExecutor : List<MessengerHandler>, IThreadDispatcherHandler<IMessageContext>, IValueHolder<Delegate>
        {
            #region Fields

            private readonly IMessenger _messenger;

            #endregion

            #region Constructors

            public MessageThreadExecutor(IMessenger messenger)
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
                for (var i = 0; i < Count; i++)
                {
                    if (this[i].Handle(messageContext) == MessengerResult.Invalid)
                        _messenger.Unsubscribe(this[i], messageContext.GetMetadataOrDefault());
                }
            }

            #endregion
        }

        #endregion
    }
}