using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Messaging.Components
{
    public interface IMessengerSubscriberDecoratorComponent : IComponent<IMessenger>
    {
        object? OnSubscribing(object subscriber, ThreadExecutionMode executionMode, IReadOnlyMetadataContext? metadata);
    }
}