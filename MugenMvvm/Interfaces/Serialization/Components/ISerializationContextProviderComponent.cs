using System.IO;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Serialization.Components
{
    public interface ISerializationContextProviderComponent : IComponent<ISerializer>
    {
        ISerializationContext? TryGetSerializationContext(ISerializer serializer, Stream stream, object request, IReadOnlyMetadataContext? metadata);

        ISerializationContext? TryGetDeserializationContext(ISerializer serializer, Stream stream, IReadOnlyMetadataContext? metadata);
    }
}