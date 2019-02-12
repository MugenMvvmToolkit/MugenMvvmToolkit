using System;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Messaging;

namespace MugenMvvm.Infrastructure.Messaging
{
    public sealed class DelegateMessengerSubscriber<TMessage> : IMessengerSubscriber
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

        public MessengerSubscriberResult Handle(object sender, object message, IMessengerContext messengerContext)
        {
            if (message is TMessage m)
            {
                _action(sender, m, messengerContext);
                return MessengerSubscriberResult.Handled;
            }

            return MessengerSubscriberResult.Ignored;
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