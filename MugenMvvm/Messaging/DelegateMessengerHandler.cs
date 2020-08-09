using System;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Messaging;

namespace MugenMvvm.Messaging
{
    public sealed class DelegateMessengerHandler<TMessage> : IMessengerHandlerRaw
    {
        #region Fields

        private readonly Action<object?, TMessage, IMessageContext> _action;

        #endregion

        #region Constructors

        public DelegateMessengerHandler(Action<object?, TMessage, IMessageContext> action)
        {
            Should.NotBeNull(action, nameof(action));
            _action = action;
        }

        #endregion

        #region Implementation of interfaces

        public bool CanHandle(Type messageType) => typeof(TMessage).IsAssignableFrom(messageType);

        public MessengerResult Handle(IMessageContext messageContext)
        {
            if (messageContext.Message is TMessage m)
            {
                _action(messageContext.Sender, m, messageContext);
                return MessengerResult.Handled;
            }

            return MessengerResult.Ignored;
        }

        #endregion
    }
}