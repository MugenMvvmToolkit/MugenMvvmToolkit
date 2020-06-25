using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Interfaces.Serialization.Components
{
    public interface ISerializerListener : IComponent<ISerializer>
    {
        void OnSerializing(ISerializer serializer, object? instance, ISerializationContext serializationContext);

        void OnSerialized(ISerializer serializer, object? instance, ISerializationContext serializationContext);

        void OnDeserializing(ISerializer serializer, object? instance, ISerializationContext serializationContext);

        void OnDeserialized(ISerializer serializer, object? instance, ISerializationContext serializationContext);
    }
}