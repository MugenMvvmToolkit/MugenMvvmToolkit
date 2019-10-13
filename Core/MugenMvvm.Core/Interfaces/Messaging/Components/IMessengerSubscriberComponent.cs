using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Messaging.Components
{
    public interface IMessengerSubscriberComponent : IComponent<IMessenger>
    {
        object? TryGetSubscriber(object subscriber, ThreadExecutionMode executionMode, IReadOnlyMetadataContext? metadata);
    }
}