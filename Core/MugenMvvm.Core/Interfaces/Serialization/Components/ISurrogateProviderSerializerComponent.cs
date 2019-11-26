using System;
using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Interfaces.Serialization.Components
{
    public interface ISurrogateProviderSerializerComponent : IComponent<ISerializer>
    {
        Type? TryGetSerializationType(Type type, ISerializationContext? serializationContext);

        object? GetObjectToSerialize(object? instance, ISerializationContext serializationContext);

        object? GetDeserializedObject(object? instance, ISerializationContext serializationContext);
    }
}