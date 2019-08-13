using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Messaging.Components
{
    public interface IMessageContextProviderComponent : IComponent<IMessenger>
    {
        IMessageContext? TryGetMessengerContext(IReadOnlyMetadataContext? metadata);
    }
}