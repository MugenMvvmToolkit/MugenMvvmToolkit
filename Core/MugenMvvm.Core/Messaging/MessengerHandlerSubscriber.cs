using System;
using System.Runtime.InteropServices;
using MugenMvvm.Collections;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Messaging;
using MugenMvvm.Messaging.Components;

namespace MugenMvvm.Messaging
{
    public sealed class MessengerHandlerSubscriber : MessengerHandlerComponent.IMessengerSubscriber
    {
        #region Fields

        private readonly Type _handlerType;
        private readonly int _hashCode;
        private readonly bool _isWeak;
        private readonly IReflectionDelegateProvider? _reflectionDelegateProvider;
        private readonly object _target;

        private static readonly CacheDictionary Cache = new CacheDictionary();

        #endregion

        #region Constructors

        public MessengerHandlerSubscriber(IMessengerHandler handler, bool isWeak, IReflectionDelegateProvider? reflectionDelegateProvider)
        {
            Should.NotBeNull(handler, nameof(handler));
            _isWeak = isWeak;
            _reflectionDelegateProvider = reflectionDelegateProvider;
            _target = isWeak ? (object)handler.ToWeakReference() : handler;
            _hashCode = handler.GetHashCode();
            _handlerType = handler.GetType();
        }

        #endregion

        #region Properties

        private object? Target
        {
            get
            {
                if (_isWeak)
                    return ((IWeakReference)_target).Target;
                return _target;
            }
        }

        #endregion

        #region Implementation of interfaces

        public bool CanHandle(Type messageType)
        {
            return GetHandler(_reflectionDelegateProvider, _handlerType, messageType) != null;
        }

        public MessengerResult Handle(IMessageContext messageContext)
        {
            var target = Target;
            if (target == null)
                return MessengerResult.Invalid;

            var handler = GetHandler(_reflectionDelegateProvider, _handlerType, messageContext.Message.GetType());
            if (handler == null)
                return MessengerResult.Ignored;
            handler.Invoke(target, messageContext.Message, messageContext);
            return MessengerResult.Handled;
        }

        #endregion

        #region Methods

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;

            var target = Target;
            if (ReferenceEquals(target, obj))
                return true;
            return obj is MessengerHandlerSubscriber handler && ReferenceEquals(target, handler.Target);
        }

        public override int GetHashCode()
        {
            return _hashCode;
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