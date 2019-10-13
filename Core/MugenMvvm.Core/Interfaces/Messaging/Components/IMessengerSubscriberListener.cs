using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Messaging.Components
{
    public interface IMessengerSubscriberListener : IComponent<IMessenger>
    {
        void OnSubscribed(IMessenger messenger, object subscriber, ThreadExecutionMode executionMode, IReadOnlyMetadataContext? metadata);

        void OnUnsubscribed(IMessenger messenger, object subscriber, IReadOnlyMetadataContext? metadata);
    }
}