using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Messaging.Components
{
    public interface IMessengerListener : IComponent<IMessenger>
    {
        void OnContextCreated(IMessenger messenger, IMessengerContext messengerContext);

        void OnSubscribed(IMessenger messenger, IMessengerSubscriber subscriber, ThreadExecutionMode executionMode, IReadOnlyMetadataContext metadata);

        void OnUnsubscribed(IMessenger messenger, IMessengerSubscriber subscriber, IReadOnlyMetadataContext metadata);
    }
}