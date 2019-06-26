using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Messaging.Components
{
    public interface IMessengerContextProviderComponent : IComponent<IMessenger>
    {
        IMessengerContext? TryGetMessengerContext(IMetadataContext? metadata);
    }
}