using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Messaging.Components
{
    public interface ISubscriberDecoratorComponent : IComponent<IMessenger>
    {
        IMessengerSubscriber OnSubscribing(IMessengerSubscriber subscriber, ThreadExecutionMode executionMode, IReadOnlyMetadataContext metadata);
    }
}