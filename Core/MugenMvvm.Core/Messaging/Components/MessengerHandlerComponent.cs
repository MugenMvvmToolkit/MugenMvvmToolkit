using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
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

namespace MugenMvvm.Messaging.Components
{
    public sealed class MessengerHandlerComponent : HashSet<MessengerHandlerComponent.HandlerSubscriber>, IMessengerSubscriberComponent, IAttachableComponent, IDetachableComponent, IHasPriority
    {
        #region Fields

        private readonly IReflectionDelegateProvider? _reflectionDelegateProvider;
        private IMessenger? _messenger;
        private static readonly CacheDictionary Cache = new CacheDictionary();
        private static readonly Func<object, object?, IMessageContext, MessengerResult> HandlerDelegate = Handle;
        private static readonly Func<object, object?, IMessageContext, MessengerResult> HandlerRawDelegate = HandleRaw;

        #endregion

        #region Constructors

        public MessengerHandlerComponent(IReflectionDelegateProvider? reflectionDelegateProvider = null)
            : base(HandlerSubscriberEqualityComparer.Instance)
        {
            _reflectionDelegateProvider = reflectionDelegateProvider;
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = MessengerComponentPriority.Subscriber;

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
                _messenger = null;
        }

        public bool TrySubscribe<TSubscriber>(in TSubscriber subscriber, ThreadExecutionMode? executionMode, IReadOnlyMetadataContext? metadata)
        {
            if (Default.IsValueType<TSubscriber>())
                return false;
            if (subscriber is IWeakReference weakReference && weakReference.Target is IMessengerHandler handler)
                return Add(new HandlerSubscriber(weakReference, handler.GetHashCode(), executionMode));
            if (subscriber is IMessengerHandler)
                return Add(new HandlerSubscriber(subscriber, subscriber.GetHashCode(), executionMode));
            return false;
        }

        public bool TryUnsubscribe<TSubscriber>(in TSubscriber subscriber, IReadOnlyMetadataContext? metadata)
        {
            if (typeof(TSubscriber) == typeof(HandlerSubscriber))
                return Remove(MugenExtensions.CastGeneric<TSubscriber, HandlerSubscriber>(subscriber));
            if (Default.IsValueType<TSubscriber>())
                return false;
            if (subscriber is IWeakReference weakReference)
            {
                var target = weakReference.Target;
                if (target == null)
                    return Remove(weakReference);
                return Remove(new HandlerSubscriber(target, subscriber.GetHashCode(), null));
            }

            if (subscriber is IMessengerHandler)
                return Remove(new HandlerSubscriber(subscriber, subscriber.GetHashCode(), null));

            return false;
        }

        public void TryUnsubscribeAll(IReadOnlyMetadataContext? metadata)
        {
            lock (this)
            {
                Clear();
            }
        }

        public IReadOnlyList<(ThreadExecutionMode, MessengerHandler)>? TryGetMessengerHandlers(Type messageType, IReadOnlyMetadataContext? metadata)
        {
            List<(ThreadExecutionMode, MessengerHandler)>? result = null;
            List<HandlerSubscriber>? toRemove = null;
            lock (this)
            {
                if (Count == 0)
                    return null;

                foreach (var handler in this)
                {
                    var subscriber = handler.GetSubscriber();
                    if (subscriber == null)
                    {
                        MugenExtensions.Add(ref toRemove, handler);
                        continue;
                    }

                    var action = GetHandler(_reflectionDelegateProvider, subscriber.GetType(), messageType);
                    if (action != null)
                        MugenExtensions.Add(ref result, (handler.ExecutionMode, new MessengerHandler(HandlerDelegate, handler.Subscriber, action)));
                    if (subscriber is IMessengerHandlerRaw handlerRaw && handlerRaw.CanHandle(messageType))
                        MugenExtensions.Add(ref result, (handler.ExecutionMode, new MessengerHandler(HandlerRawDelegate, handler.Subscriber)));
                }
            }

            if (toRemove != null)
            {
                for (var i = 0; i < toRemove.Count; i++)
                    _messenger.Unsubscribe(toRemove[i], metadata);
            }

            return result;
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

        private static MessengerResult Handle(object subscriber, object? handler, IMessageContext context)
        {
            if (subscriber is IWeakReference weakReference)
                subscriber = weakReference.Target;
            if (subscriber == null || handler == null)
                return MessengerResult.Invalid;

            ((Action<object?, object?, IMessageContext>) handler).Invoke(subscriber, context.Message, context);
            return MessengerResult.Handled;
        }

        private static MessengerResult HandleRaw(object subscriber, object? _, IMessageContext context)
        {
            if (subscriber is IWeakReference weakReference)
                subscriber = weakReference.Target;
            if (subscriber == null)
                return MessengerResult.Invalid;
            return ((IMessengerHandlerRaw) subscriber).Handle(context);
        }

        private new bool Add(HandlerSubscriber subscriber)
        {
            lock (this)
            {
                return base.Add(subscriber);
            }
        }

        private new bool Remove(HandlerSubscriber subscriber)
        {
            lock (this)
            {
                return base.Remove(subscriber);
            }
        }

        private bool Remove(IWeakReference subscriber)
        {
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

                return base.Remove(handler);
            }
        }

        public static Action<object?, object?, IMessageContext>? GetHandler(IReflectionDelegateProvider? reflectionDelegateProvider, Type handlerType, Type messageType)
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

            public HandlerSubscriber(object subscriber, int hashCode, ThreadExecutionMode? executionMode)
            {
                Subscriber = subscriber;
                Hash = hashCode;
                ExecutionMode = executionMode ?? ThreadExecutionMode.Current;
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