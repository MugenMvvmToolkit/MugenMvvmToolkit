using System;
using System.Reflection;
using MugenMvvm.Constants;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Messaging;
using MugenMvvm.Messaging.Components;

namespace MugenMvvm.Messaging
{
    public sealed class WeakDelegateMessengerSubscriber<TTarget, TMessage> : MessengerHandlerComponent.IMessengerSubscriber
        where TTarget : class
    {
        #region Fields

        private readonly Action<TTarget, TMessage, IMessageContext> _action;
        private readonly IWeakReference _reference;

        #endregion

        #region Constructors

        public WeakDelegateMessengerSubscriber(TTarget target, Action<TTarget, TMessage, IMessageContext> action)
        {
            Should.NotBeNull(target, nameof(target));
            Should.NotBeNull(action, nameof(action));
            _reference = target.ToWeakReference();
            _action = action;
        }

        public WeakDelegateMessengerSubscriber(Action<TMessage, IMessageContext> action)
        {
            Should.NotBeNull(action, nameof(action));
            Should.BeSupported(action.Target != null, MessageConstants.StaticDelegateCannotBeWeak);
            Should.BeSupported(!action.Target!.GetType().IsAnonymousClass(), MessageConstants.AnonymousDelegateCannotBeWeak);
            _reference = action.Target.ToWeakReference();
            _action = action.GetMethodInfo().GetMethodInvoker<Action<TTarget, TMessage, IMessageContext>>();
        }

        #endregion

        #region Implementation of interfaces

        public MessengerResult Handle(IMessageContext messageContext)
        {
            var target = (TTarget?)_reference.Target;
            if (target == null)
                return MessengerResult.Invalid;

            if (!(messageContext.Message is TMessage m))
                return MessengerResult.Ignored;

            _action(target, m, messageContext);
            return MessengerResult.Handled;

        }

        #endregion

        #region Methods

        public override string ToString()
        {
            return _reference.Target?.ToString() ?? base.ToString();
        }

        #endregion
    }
}