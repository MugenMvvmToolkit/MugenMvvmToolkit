using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Serialization.Components
{
    public interface ISerializationContextProviderComponent : IComponent<ISerializer>
    {
        ISerializationContext? TryGetSerializationContext(IReadOnlyMetadataContext? metadata);
    }
}