using System;
using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Interfaces.Serialization
{
    public interface ISurrogateProviderComponent : IComponent<ISerializer>
    {
        Type? TryGetSerializationType(ISerializer serializer, Type type);

        object? GetObjectToSerialize(ISerializer serializer, object? instance, ISerializationContext serializationContext);

        object? GetDeserializedObject(ISerializer serializer, object? instance, ISerializationContext serializationContext);
    }
}