using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Messaging.Components
{
    public interface IMessengerListener : IComponent<IMessenger>
    {
        void OnContextCreated(IMessenger messenger, object message, IMessageContext messageContext);

        void OnSubscribed(IMessenger messenger, object subscriber, ThreadExecutionMode executionMode, IReadOnlyMetadataContext? metadata);

        void OnUnsubscribed(IMessenger messenger, object subscriber, IReadOnlyMetadataContext? metadata);

        void OnHandling(object subscriber, IMessageContext messageContext);

        void OnHandled(MessengerResult? result, object subscriber, IMessageContext messageContext);
    }
}