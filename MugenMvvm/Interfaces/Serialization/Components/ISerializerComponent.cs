using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Interfaces.Serialization.Components
{
    public interface ISerializerComponent : IComponent<ISerializer>
    {
        bool TrySerialize(ISerializer serializer, object request, ISerializationContext serializationContext);

        bool TryDeserialize(ISerializer serializer, ISerializationContext serializationContext, out object? value);
    }
}