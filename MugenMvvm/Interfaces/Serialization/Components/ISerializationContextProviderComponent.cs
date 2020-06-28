using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Serialization.Components
{
    public interface ISerializationContextProviderComponent : IComponent<ISerializer>
    {
        ISerializationContext? TryGetSerializationContext<TRequest>(ISerializer serializer, in TRequest request, IReadOnlyMetadataContext? metadata);

        ISerializationContext? TryGetDeserializationContext(ISerializer serializer, IReadOnlyMetadataContext? metadata);
    }
}