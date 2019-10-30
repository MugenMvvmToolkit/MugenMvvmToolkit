using System;
using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Collections;
using MugenMvvm.Enums;
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
        private readonly object _target;

        private static readonly CacheDictionary Cache = new CacheDictionary();

        #endregion

        #region Constructors

        public MessengerHandlerSubscriber(IMessengerHandler handler, bool isWeak)
        {
            Should.NotBeNull(handler, nameof(handler));
            _isWeak = isWeak;
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

        public bool CanHandle(IMessageContext messageContext)
        {
            return GetHandlers(_handlerType, messageContext.Message) != null;
        }

        public MessengerResult Handle(IMessageContext messageContext)
        {
            var target = Target;
            if (target == null)
                return MessengerResult.Invalid;

            var handlers = GetHandlers(_handlerType, messageContext.Message);
            if (handlers == null)
                return MessengerResult.Ignored;

            var args = new[] { messageContext.Message, messageContext };
            for (var i = 0; i < handlers.Count; i++)
                handlers[i].Invoke(target, args);

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

        private static List<Func<object?, object?[], object?>>? GetHandlers(Type handlerType, object message)
        {
            var key = new CacheKey(handlerType, message.GetType());
            lock (Cache)
            {
                if (!Cache.TryGetValue(key, out var items))
                {
                    var interfaces = key.HandlerType
                        .GetInterfaces()
                        .Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IMessengerHandler<>));
                    foreach (var @interface in interfaces)
                    {
                        var typeMessage = @interface.GetGenericArguments()[0];
                        var method = @interface.GetMethod(nameof(IMessengerHandler<object>.Handle), BindingFlagsEx.InstancePublic);
                        if (method != null && typeMessage.IsAssignableFrom(key.MessageType))
                        {
                            if (items == null)
                                items = new List<Func<object?, object?[], object?>>(2);
                            items.Add(method.GetMethodInvoker());
                        }
                    }

                    Cache[key] = items;
                }

                return items;
            }
        }

        #endregion

        #region Nested types

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

        private sealed class CacheDictionary : LightDictionary<CacheKey, List<Func<object?, object?[], object?>>?>
        {
            #region Constructors

            public CacheDictionary() : base(59)
            {
            }

            #endregion

            #region Methods

            protected override int GetHashCode(CacheKey key)
            {
                unchecked
                {
                    return key.HandlerType.GetHashCode() * 397 ^ key.MessageType.GetHashCode();
                }
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