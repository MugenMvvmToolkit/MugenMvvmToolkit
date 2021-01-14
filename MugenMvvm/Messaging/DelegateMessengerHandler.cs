﻿using System;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Messaging;

namespace MugenMvvm.Messaging
{
    public sealed class DelegateMessengerHandler<TMessage> : IMessengerHandler
    {
        private readonly Action<object?, TMessage, IMessageContext> _action;

        public DelegateMessengerHandler(Action<object?, TMessage, IMessageContext> action)
        {
            Should.NotBeNull(action, nameof(action));
            _action = action;
        }

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
    }
}