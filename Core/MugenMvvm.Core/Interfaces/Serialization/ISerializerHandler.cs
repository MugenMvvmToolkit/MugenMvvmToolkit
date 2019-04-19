using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Interfaces.Serialization
{
    public interface ISerializerHandler : IHasPriority
    {
        void OnSerializing(ISerializer serializer, object? instance, ISerializationContext serializationContext);

        void OnSerialized(ISerializer serializer, object? instance, ISerializationContext serializationContext);

        void OnDeserializing(ISerializer serializer, object? instance, ISerializationContext serializationContext);

        void OnDeserialized(ISerializer serializer, object? instance, ISerializationContext serializationContext);
    }
}