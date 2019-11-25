using System;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Messaging;
using MugenMvvm.Messaging.Components;

namespace MugenMvvm.Messaging
{
    public sealed class DelegateMessengerSubscriber<TMessage> : MessengerHandlerComponent.IMessengerSubscriber
    {
        #region Fields

        private readonly Action<TMessage, IMessageContext> _action;

        #endregion

        #region Constructors

        public DelegateMessengerSubscriber(Action<TMessage, IMessageContext> action)
        {
            Should.NotBeNull(action, nameof(action));
            _action = action;
        }

        #endregion

        #region Implementation of interfaces

        public bool CanHandle(Type messageType)
        {
            return typeof(TMessage).IsAssignableFrom(messageType);
        }

        public MessengerResult Handle(IMessageContext messageContext)
        {
            if (!(messageContext.Message is TMessage m))
                return MessengerResult.Ignored;

            _action(m, messageContext);
            return MessengerResult.Handled;
        }

        #endregion

        #region Methods

        public override string ToString()
        {
            return _action.ToString();
        }

        #endregion
    }
}