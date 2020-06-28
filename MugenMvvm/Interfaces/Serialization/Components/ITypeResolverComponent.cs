using System;
using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Interfaces.Serialization.Components
{
    public interface ITypeResolverComponent : IComponent<ISerializer>
    {
        Type? TryResolveType(string assemblyName, string typeName, ISerializationContext? serializationContext);

        bool TryResolveName(Type serializedType, ISerializationContext? serializationContext, out string? assemblyName, out string? typeName);
    }
}