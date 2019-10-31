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

        public WeakDelegateMessengerSubscriber(IWeakReference reference, Action<TTarget, TMessage, IMessageContext> action)
        {
            Should.NotBeNull(reference, nameof(reference));
            Should.NotBeNull(action, nameof(action));
            _reference = reference;
            _action = action;
        }

        #endregion

        #region Implementation of interfaces

        public bool CanHandle(IMessageContext messageContext)
        {
            return messageContext.Message is TMessage;
        }

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