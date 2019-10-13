using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Messaging;
using MugenMvvm.Interfaces.Messaging.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Messaging.Components
{
    public sealed class MessengerHandlerComponent : IMessengerHandlerComponent, IMessengerSubscriberComponent
    {
        #region Fields

        public static readonly MessengerHandlerComponent Instance = new MessengerHandlerComponent();

        #endregion

        #region Implementation of interfaces

        public bool CanHandle(object subscriber, IMessageContext messageContext)
        {
            if (subscriber is IMessengerSubscriber messengerSubscriber)
                return messengerSubscriber.CanHandle(messageContext);
            return false;
        }

        public MessengerResult? TryHandle(object subscriber, IMessageContext messageContext)
        {
            if (subscriber is IMessengerSubscriber messengerSubscriber)
                return messengerSubscriber.Handle(messageContext);
            return null;
        }

        public object? TryGetSubscriber(object subscriber, ThreadExecutionMode executionMode, IReadOnlyMetadataContext? metadata)
        {
            return subscriber as IMessengerSubscriber;
        }

        #endregion

        #region Nested types

        public interface IMessengerSubscriber
        {
            bool CanHandle(IMessageContext messageContext);

            MessengerResult Handle(IMessageContext messageContext);
        }

        #endregion
    }
}