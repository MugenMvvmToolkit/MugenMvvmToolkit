using System.IO;
using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Interfaces.Serialization.Components
{
    public interface ISerializerComponent : IComponent<ISerializer>
    {
        bool TrySerialize(ISerializer serializer, Stream stream, object request, ISerializationContext serializationContext);

        bool TryDeserialize(ISerializer serializer, Stream stream, ISerializationContext serializationContext, out object? value);
    }
}