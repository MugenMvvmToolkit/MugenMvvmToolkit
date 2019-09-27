using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MugenMvvm.Attributes;
using MugenMvvm.Collections;
using MugenMvvm.Collections.Internal;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Messaging;
using MugenMvvm.Internal;
using MugenMvvm.Messaging.Components;

namespace MugenMvvm.Messaging
{
    public sealed class MessengerHandlerSubscriber : MessengerHandlerComponent.IMessengerSubscriber
    {
        #region Fields

        private readonly int _hashCode;
        private readonly bool _isWeak;
        private readonly object _target;

        private static readonly MethodInfo InvokeMethodInfo = GetInvokeMethod();

        private static readonly TypeLightDictionary<Func<object?, object?[], object?>> MessageTypeToDelegate =
            new TypeLightDictionary<Func<object?, object?[], object?>>(23);

        #endregion

        #region Constructors

        public MessengerHandlerSubscriber(IMessengerHandler handler, bool isWeak)
        {
            Should.NotBeNull(handler, nameof(handler));
            _isWeak = isWeak;
            _target = isWeak ? (object)handler.ToWeakReference() : handler;
            _hashCode = handler.GetHashCode();
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

        public MessengerResult Handle(IMessageContext messageContext)
        {
            var target = Target;
            if (target == null)
                return MessengerResult.Invalid;

            Func<object?, object?[], object?> func;
            lock (MessageTypeToDelegate)
            {
                var msgType = messageContext.Message.GetType();
                if (!MessageTypeToDelegate.TryGetValue(msgType, out func))
                {
                    func = InvokeMethodInfo.MakeGenericMethod(msgType).GetMethodInvoker();
                    MessageTypeToDelegate[msgType] = func;
                }
            }

            if (func.Invoke(null, new[] { target, messageContext }) == null)
                return MessengerResult.Ignored;
            return MessengerResult.Handled;
        }

        #endregion

        #region Methods

        private static MethodInfo GetInvokeMethod()
        {
            var m = typeof(MessengerHandlerSubscriber)
                .GetMethodsUnified(MemberFlags.StaticOnly)
                .FirstOrDefault(info => nameof(Invoke).Equals(info.Name));
            Should.BeSupported(m != null, nameof(InvokeMethodInfo));
            return m!;
        }

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

        [Preserve(Conditional = true)]
        internal static object? Invoke<T>(object target, IMessageContext messageContext)
        {
            if (!(target is IMessengerHandler<T> handler))
                return null;

            handler.Handle((T)messageContext.Message, messageContext);
            return handler;

        }

        #endregion
    }
}