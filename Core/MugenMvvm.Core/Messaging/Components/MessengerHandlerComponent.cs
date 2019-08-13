using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Messaging;
using MugenMvvm.Interfaces.Messaging.Components;

namespace MugenMvvm.Messaging.Components
{
    public sealed class MessengerHandlerComponent : IMessengerHandlerComponent
    {
        #region Fields

        public static readonly MessengerHandlerComponent Instance = new MessengerHandlerComponent();

        #endregion

        #region Implementation of interfaces

        public MessengerResult? TryHandle(object subscriber, IMessageContext messageContext)
        {
            if (subscriber is IMessengerSubscriber messengerSubscriber)
                return messengerSubscriber.Handle(messageContext);
            return null;
        }

        #endregion

        #region Nested types

        public interface IMessengerSubscriber
        {
            MessengerResult Handle(IMessageContext messageContext);
        }

        #endregion
    }
}