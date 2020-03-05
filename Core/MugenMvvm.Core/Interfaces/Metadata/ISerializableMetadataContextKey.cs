using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Serialization;

namespace MugenMvvm.Interfaces.Metadata
{
    public interface ISerializableMetadataContextKey : IMetadataContextKey
    {
        bool CanSerialize(object? item, ISerializationContext serializationContext);

        object? Serialize(object? item, ISerializationContext serializationContext);

        object? Deserialize(object? item, ISerializationContext serializationContext);
    }
}