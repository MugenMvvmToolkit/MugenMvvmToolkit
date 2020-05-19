using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MugenMvvm.Attributes;
using MugenMvvm.Collections;
using MugenMvvm.Constants;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Messaging;
using MugenMvvm.Interfaces.Messaging.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;

namespace MugenMvvm.Messaging.Components
{
    public sealed class MessengerHandlerSubscriber : HashSet<MessengerHandlerSubscriber.HandlerSubscriber>, IMessengerSubscriberComponent, IAttachableComponent, IDetachableComponent, IHasPriority
    {
        #region Fields

        private readonly IReflectionDelegateProvider? _reflectionDelegateProvider;
        private IMessenger? _messenger;
        private static readonly CacheDictionary Cache = new CacheDictionary();
        private static readonly Func<object, object?, IMessageContext, MessengerResult> HandlerDelegate = Handle;
        private static readonly Func<object, object?, IMessageContext, MessengerResult> HandlerRawDelegate = HandleRaw;

        #endregion

        #region Constructors

        public MessengerHandlerSubscriber(IReflectionDelegateProvider? reflectionDelegateProvider = null)
            : base(HandlerSubscriberEqualityComparer.Instance)
        {
            _reflectionDelegateProvider = reflectionDelegateProvider;
            ExecutionMode = ThreadExecutionMode.Current;
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = MessengerComponentPriority.Subscriber;

        public ThreadExecutionMode ExecutionMode { get; set; }

        #endregion

        #region Implementation of interfaces

        bool IAttachableComponent.OnAttaching(object owner, IReadOnlyMetadataContext? metadata)
        {
            return true;
        }

        void IAttachableComponent.OnAttached(object owner, IReadOnlyMetadataContext? metadata)
        {
            _messenger = owner as IMessenger;
        }

        bool IDetachableComponent.OnDetaching(object owner, IReadOnlyMetadataContext? metadata)
        {
            return true;
        }

        void IDetachableComponent.OnDetached(object owner, IReadOnlyMetadataContext? metadata)
        {
            if (ReferenceEquals(_messenger, owner))
            {
                TryUnsubscribeAll(metadata);
                _messenger = null;
            }
        }

        public bool TrySubscribe<TSubscriber>(in TSubscriber subscriber, ThreadExecutionMode? executionMode, IReadOnlyMetadataContext? metadata)
        {
            if (_messenger == null)
                return false;
            if (Default.IsValueType<TSubscriber>())
                return false;
            if (subscriber is IWeakReference weakReference && weakReference.Target is IMessengerHandler handler)
                return Add(new HandlerSubscriber(weakReference, RuntimeHelpers.GetHashCode(handler), executionMode ?? ExecutionMode), metadata);
            if (subscriber is IMessengerHandler)
                return Add(new HandlerSubscriber(subscriber, RuntimeHelpers.GetHashCode(subscriber), executionMode ?? ExecutionMode), metadata);
            return false;
        }

        public bool TryUnsubscribe<TSubscriber>(in TSubscriber subscriber, IReadOnlyMetadataContext? metadata)
        {
            if (_messenger == null)
                return false;
            if (typeof(TSubscriber) == typeof(HandlerSubscriber))
                return Remove(MugenExtensions.CastGeneric<TSubscriber, HandlerSubscriber>(subscriber), metadata);
            if (Default.IsValueType<TSubscriber>())
                return false;
            if (subscriber is IWeakReference weakReference)
            {
                var target = weakReference.Target;
                if (target == null)
                    return Remove(weakReference, metadata);
                return Remove(new HandlerSubscriber(target, RuntimeHelpers.GetHashCode(target), ExecutionMode), metadata);
            }

            if (subscriber is IMessengerHandler)
                return Remove(new HandlerSubscriber(subscriber, RuntimeHelpers.GetHashCode(subscriber), ExecutionMode), metadata);

            return false;
        }

        public void TryUnsubscribeAll(IReadOnlyMetadataContext? metadata)
        {
            bool invalidate;
            lock (this)
            {
                invalidate = Count != 0;
                Clear();
            }

            if (invalidate)
                _messenger.TryInvalidateCache(metadata);
        }

        public IReadOnlyList<MessengerHandler>? TryGetMessengerHandlers(Type messageType, IReadOnlyMetadataContext? metadata)
        {
            LazyList<MessengerHandler> result = default;
            LazyList<HandlerSubscriber> toRemove = default;
            lock (this)
            {
                if (Count == 0)
                    return null;

                foreach (var handler in this)
                {
                    var subscriber = handler.GetSubscriber();
                    if (subscriber == null)
                    {
                        toRemove.Add(handler);
                        continue;
                    }

                    var action = GetHandler(_reflectionDelegateProvider, subscriber.GetType(), messageType);
                    if (action != null)
                        result.Add(new MessengerHandler(HandlerDelegate, handler.Subscriber, handler.ExecutionMode, action));
                    if (subscriber is IMessengerHandlerRaw handlerRaw && handlerRaw.CanHandle(messageType))
                        result.Add(new MessengerHandler(HandlerRawDelegate, handler.Subscriber, handler.ExecutionMode));
                }
            }

            var toRemoveList = toRemove.List;
            var messenger = _messenger;
            if (toRemoveList != null && messenger != null)
            {
                for (var i = 0; i < toRemoveList.Count; i++)
                    messenger.Unsubscribe(toRemoveList[i], metadata);
            }

            return result.List;
        }

        public IReadOnlyList<MessengerSubscriberInfo>? TryGetSubscribers(IReadOnlyMetadataContext? metadata)
        {
            lock (this)
            {
                if (Count == 0)
                    return null;
                return this.ToArray(subscriber => new MessengerSubscriberInfo(subscriber.Subscriber, subscriber.ExecutionMode));
            }
        }

        #endregion

        #region Methods

        [Preserve(Conditional = true)]
        private static MessengerResult Handle(object subscriber, object? handler, IMessageContext context)
        {
            if (subscriber is IWeakReference weakReference)
                subscriber = weakReference.Target!;
            if (subscriber == null || handler == null)
                return MessengerResult.Invalid;

            ((Action<object?, object?, IMessageContext>) handler).Invoke(subscriber, context.Message, context);
            return MessengerResult.Handled;
        }

        [Preserve(Conditional = true)]
        private static MessengerResult HandleRaw(object subscriber, object? _, IMessageContext context)
        {
            if (subscriber is IWeakReference weakReference)
                subscriber = weakReference.Target!;
            if (subscriber == null)
                return MessengerResult.Invalid;
            return ((IMessengerHandlerRaw) subscriber).Handle(context);
        }

        private bool Add(HandlerSubscriber subscriber, IReadOnlyMetadataContext? metadata)
        {
            bool result;
            lock (this)
            {
                result = Add(subscriber);
            }

            if (result)
                _messenger.TryInvalidateCache(metadata);
            return result;
        }

        private bool Remove(HandlerSubscriber subscriber, IReadOnlyMetadataContext? metadata)
        {
            bool result;
            lock (this)
            {
                result = Remove(subscriber);
            }

            if (result)
                _messenger.TryInvalidateCache(metadata);
            return result;
        }

        private bool Remove(IWeakReference subscriber, IReadOnlyMetadataContext? metadata)
        {
            bool result;
            lock (this)
            {
                HandlerSubscriber handler = default;
                foreach (var item in this)
                {
                    if (ReferenceEquals(item.Subscriber, subscriber))
                    {
                        handler = item;
                        break;
                    }
                }

                result = Remove(handler);
            }

            if (result)
                _messenger.TryInvalidateCache(metadata);
            return result;
        }

        private static Action<object?, object?, IMessageContext>? GetHandler(IReflectionDelegateProvider? reflectionDelegateProvider, Type handlerType, Type messageType)
        {
            var key = new CacheKey(handlerType, messageType);
            lock (Cache)
            {
                if (!Cache.TryGetValue(key, out var action))
                {
                    var interfaces = key.HandlerType.GetInterfaces();
                    for (var index = 0; index < interfaces.Length; index++)
                    {
                        var @interface = interfaces[index];
                        if (!@interface.IsGenericType || @interface.GetGenericTypeDefinition() != typeof(IMessengerHandler<>))
                            continue;
                        var typeMessage = @interface.GetGenericArguments()[0];
                        var method = @interface.GetMethod(nameof(IMessengerHandler<object>.Handle), BindingFlagsEx.InstancePublic);
                        if (method != null && typeMessage.IsAssignableFrom(key.MessageType))
                            action += method.GetMethodInvoker<Action<object?, object?, IMessageContext>>(reflectionDelegateProvider);
                    }

                    Cache[key] = action;
                }

                return action;
            }
        }

        #endregion

        #region Nested types

        private sealed class HandlerSubscriberEqualityComparer : IEqualityComparer<HandlerSubscriber>
        {
            #region Fields

            public static readonly IEqualityComparer<HandlerSubscriber> Instance = new HandlerSubscriberEqualityComparer();

            #endregion

            #region Constructors

            private HandlerSubscriberEqualityComparer()
            {
            }

            #endregion

            #region Implementation of interfaces

            public bool Equals(HandlerSubscriber x, HandlerSubscriber y)
            {
                return ReferenceEquals(x.Subscriber, y.Subscriber) || ReferenceEquals(x.GetSubscriber(), y.GetSubscriber());
            }

            public int GetHashCode(HandlerSubscriber obj)
            {
                return obj.Hash;
            }

            #endregion
        }

        [StructLayout(LayoutKind.Auto)]
        public readonly struct HandlerSubscriber
        {
            #region Fields

            public readonly ThreadExecutionMode ExecutionMode;
            public readonly int Hash;
            public readonly object Subscriber;

            #endregion

            #region Constructors

            public HandlerSubscriber(object subscriber, int hashCode, ThreadExecutionMode executionMode)
            {
                Subscriber = subscriber;
                Hash = hashCode;
                ExecutionMode = executionMode;
            }

            #endregion

            #region Methods

            public object? GetSubscriber()
            {
                if (Subscriber is IWeakReference w)
                    return w.Target;
                return Subscriber;
            }

            #endregion
        }

        [StructLayout(LayoutKind.Auto)]
        private readonly struct CacheKey
        {
            #region Fields

            public readonly Type HandlerType;
            public readonly Type MessageType;

            #endregion

            #region Constructors

            public CacheKey(Type handlerType, Type messageType)
            {
                HandlerType = handlerType;
                MessageType = messageType;
            }

            #endregion
        }

        private sealed class CacheDictionary : LightDictionary<CacheKey, Action<object?, object?, IMessageContext>?>
        {
            #region Constructors

            public CacheDictionary() : base(59)
            {
            }

            #endregion

            #region Methods

            protected override int GetHashCode(CacheKey key)
            {
                return HashCode.Combine(key.HandlerType, key.MessageType);
            }

            protected override bool Equals(CacheKey x, CacheKey y)
            {
                return x.HandlerType == y.HandlerType && x.MessageType == y.MessageType;
            }

            #endregion
        }

        #endregion
    }
}