using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MugenMvvm.Attributes;
using MugenMvvm.Enums;
using MugenMvvm.Infrastructure.Internal;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Messaging;

namespace MugenMvvm.Infrastructure.Messaging
{
    public sealed class MessengerHandlerSubscriber : IMessengerSubscriber
    {
        #region Fields

        private readonly int _hashCode;
        private readonly IWeakReference _reference;

        private static readonly MethodInfo InvokeMethodInfo = GetInvokeMethod();
        private static readonly Dictionary<Type, Func<object?, object?[], object?>> MessageTypeToDelegate =
            new Dictionary<Type, Func<object?, object?[], object?>>(MemberInfoEqualityComparer.Instance);

        #endregion

        #region Constructors

        public MessengerHandlerSubscriber(IMessengerHandler handler)
        {
            Should.NotBeNull(handler, nameof(handler));
            _reference = Service<IWeakReferenceProvider>.Instance.GetWeakReference(handler, Default.Metadata);
            _hashCode = handler.GetHashCode();
        }

        #endregion

        #region Properties

        private object? Target => _reference.Target;

        #endregion

        #region Implementation of interfaces

        public bool Equals(IMessengerSubscriber other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;
            return other is MessengerHandlerSubscriber handler && ReferenceEquals(Target, handler.Target);
        }

        public MessengerSubscriberResult Handle(object sender, object message, IMessengerContext messengerContext)
        {
            var target = _reference.Target;
            if (target == null)
                return MessengerSubscriberResult.Invalid;

            Func<object?, object?[], object?> func;
            lock (MessageTypeToDelegate)
            {
                var msgType = message.GetType();
                if (!MessageTypeToDelegate.TryGetValue(msgType, out func))
                {
                    func = InvokeMethodInfo.MakeGenericMethod(msgType).GetMethodInvoker();
                    MessageTypeToDelegate[msgType] = func;
                }
            }

            if (func.Invoke(null, new[] { target, sender, message, messengerContext }) == null)
                return MessengerSubscriberResult.Ignored;
            return MessengerSubscriberResult.Handled;
        }

        #endregion

        #region Methods

        private static MethodInfo GetInvokeMethod()
        {
            var m = typeof(MessengerHandlerSubscriber)
                .GetMethodsUnified(MemberFlags.StaticOnly)
                .FirstOrDefault(info => nameof(Invoke).Equals(info.Name));
            Should.BeSupported(m != null, nameof(InvokeMethodInfo));
            return m;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj is MessengerHandlerSubscriber handler)
                return Equals(handler);
            return false;
        }

        public override int GetHashCode()
        {
            return _hashCode;
        }

        [Preserve(Conditional = true)]
        internal static object? Invoke<T>(object target, object sender, T message, IMessengerContext messengerContext)
        {
            if (target is IMessengerHandler<T> handler)
            {
                handler.Handle(sender, message, messengerContext);
                return handler;
            }

            return null;
        }

        #endregion
    }
}