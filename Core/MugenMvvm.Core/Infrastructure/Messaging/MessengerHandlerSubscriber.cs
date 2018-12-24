using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MugenMvvm.Attributes;
using MugenMvvm.Interfaces;
using MugenMvvm.Interfaces.Messaging;
using MugenMvvm.Models;

namespace MugenMvvm.Infrastructure.Messaging
{
    public class MessengerHandlerSubscriber : IMessengerSubscriber
    {
        #region Fields

        private readonly int _hashCode;
        private readonly WeakReference _reference;
        private static readonly MethodInfo InvokeMethodInfo;
        private static readonly Dictionary<Type, Func<object?, object?[], object?>> MessageTypeToDelegate;

        #endregion

        #region Constructors

        static MessengerHandlerSubscriber()
        {
            InvokeMethodInfo = typeof(MessengerHandlerSubscriber)
                .GetMethodsUnified(MemberFlags.StaticOnly)
                .FirstOrDefault(info => nameof(Invoke).Equals(info.Name));
            Should.BeSupported(InvokeMethodInfo != null, nameof(InvokeMethodInfo));

            MessageTypeToDelegate = new Dictionary<Type, Func<object?, object?[], object?>>();
        }

        public MessengerHandlerSubscriber(IMessengerHandler handler)
        {
            Should.NotBeNull(handler, nameof(handler));
            _reference = MugenExtensions.GetWeakReference(handler);
            _hashCode = handler.GetHashCode();
        }

        #endregion

        #region Properties

        private object Target => _reference.Target;

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

        public SubscriberResult Handle(object sender, object message, IMessengerContext messengerContext)
        {
            var target = _reference.Target;
            if (target == null)
                return SubscriberResult.Invalid;

            Func<object?, object?[], object?> func;
            lock (MessageTypeToDelegate)
            {
                var msgType = message.GetType();
                if (!MessageTypeToDelegate.TryGetValue(msgType, out func))
                {
                    func = Singleton<IReflectionManager>.Instance.GetMethodDelegate(InvokeMethodInfo.MakeGenericMethod(msgType));
                    MessageTypeToDelegate[msgType] = func;
                }
            }

            if (func.Invoke(null, new[] {target, sender, message, messengerContext}) == null)
                return SubscriberResult.Ignored;
            return SubscriberResult.Handled;
        }

        #endregion

        #region Methods

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