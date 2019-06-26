using System;
using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Interfaces.Serialization
{
    public interface ITypeResolverComponent : IComponent<ISerializer>
    {
        Type? TryResolveType(ISerializer serializer, string assemblyName, string typeName);

        bool TryResolveName(ISerializer serializer, Type serializedType, out string? assemblyName, out string? typeName);
    }
}