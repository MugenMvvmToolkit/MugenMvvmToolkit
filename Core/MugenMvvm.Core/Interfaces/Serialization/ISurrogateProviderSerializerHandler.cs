using System;

namespace MugenMvvm.Interfaces.Serialization
{
    public interface ISurrogateProviderSerializerHandler : ISerializerHandler
    {
        Type? TryGetSerializationType(ISerializer serializer, Type type);

        object? GetObjectToSerialize(ISerializer serializer, object? instance, ISerializationContext serializationContext);

        object? GetDeserializedObject(ISerializer serializer, object? instance, ISerializationContext serializationContext);
    }
}