using System;
using System.Reflection;
using MugenMvvm.Constants;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Messaging;

namespace MugenMvvm.Messaging
{
    public sealed class WeakDelegateMessengerHandler<TTarget, TMessage> : IMessengerHandlerRaw
        where TTarget : class
    {
        #region Fields

        private readonly Action<TTarget, object?, TMessage, IMessageContext> _action;
        private readonly IWeakReference _reference;

        #endregion

        #region Constructors

        public WeakDelegateMessengerHandler(TTarget target, Action<TTarget, object?, TMessage, IMessageContext> action)
        {
            Should.NotBeNull(target, nameof(target));
            Should.NotBeNull(action, nameof(action));
            _reference = target.ToWeakReference();
            _action = action;
        }

        public WeakDelegateMessengerHandler(Action<object?, TMessage, IMessageContext> action)
        {
            Should.NotBeNull(action, nameof(action));
            Should.BeSupported(action.Target != null, MessageConstant.StaticDelegateCannotBeWeak);
            Should.BeSupported(!action.Target.GetType().IsAnonymousClass(), MessageConstant.AnonymousDelegateCannotBeWeak);
            _reference = action.Target.ToWeakReference();
            _action = action.GetMethodInfo()!.GetMethodInvoker<Action<TTarget, object, TMessage, IMessageContext>>()!;
        }

        #endregion

        #region Implementation of interfaces

        public bool CanHandle(Type messageType) => typeof(TMessage).IsAssignableFrom(messageType);

        public MessengerResult Handle(IMessageContext messageContext)
        {
            var target = (TTarget?) _reference.Target;
            if (target == null)
                return MessengerResult.Invalid;
            if (messageContext.Message is TMessage m)
            {
                _action(target, messageContext.Sender, m, messageContext);
                return MessengerResult.Handled;
            }

            return MessengerResult.Ignored;
        }

        #endregion
    }
}