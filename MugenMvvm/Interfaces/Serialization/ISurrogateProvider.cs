using System;

namespace MugenMvvm.Interfaces.Serialization
{
    public interface ISurrogateProvider
    {
        Type SurrogateType { get; }

        object? GetObjectToSerialize(object? instance, ISerializationContext serializationContext);

        object? GetDeserializedObject(object? instance, ISerializationContext serializationContext);
    }
}