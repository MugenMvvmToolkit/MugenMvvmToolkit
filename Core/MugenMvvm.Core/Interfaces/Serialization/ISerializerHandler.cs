using System;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Interfaces.Serialization
{
    public interface ISerializerHandler : IHasPriority
    {
        Type? TryGetSerializationType(Type type);

        object? Serialize(object? instance, ISerializationContext serializationContext);

        object? Deserialize(object? instance, ISerializationContext serializationContext);
    }
}