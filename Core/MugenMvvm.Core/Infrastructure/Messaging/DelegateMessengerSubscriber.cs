using System;
using MugenMvvm.Interfaces.Messaging;
using MugenMvvm.Models;

namespace MugenMvvm.Infrastructure.Messaging
{
    public class DelegateMessengerSubscriber<TMessage> : IMessengerSubscriber
    {
        #region Fields

        private readonly Action<object, TMessage, IMessengerContext> _action;

        #endregion

        #region Constructors

        public DelegateMessengerSubscriber(Action<object, TMessage, IMessengerContext> action)
        {
            Should.NotBeNull(action, nameof(action));
            _action = action;
        }

        #endregion

        #region Implementation of interfaces

        public bool Equals(IMessengerSubscriber other)
        {
            return ReferenceEquals(other, this);
        }

        public SubscriberResult Handle(object sender, object message, IMessengerContext messengerContext)
        {
            if (message is TMessage m)
            {
                _action(sender, m, messengerContext);
                return SubscriberResult.Handled;
            }

            return SubscriberResult.Ignored;
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