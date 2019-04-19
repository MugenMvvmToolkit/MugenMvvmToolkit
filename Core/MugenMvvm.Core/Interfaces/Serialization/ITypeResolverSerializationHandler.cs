using System;

namespace MugenMvvm.Interfaces.Serialization
{
    public interface ITypeResolverSerializationHandler : ISerializerHandler
    {
        Type? TryResolveType(ISerializer serializer, string assemblyName, string typeName);

        bool TryResolveName(ISerializer serializer, Type serializedType, out string? assemblyName, out string? typeName);
    }
}