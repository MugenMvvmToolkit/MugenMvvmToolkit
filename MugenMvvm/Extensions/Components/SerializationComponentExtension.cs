using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Serialization;
using MugenMvvm.Interfaces.Serialization.Components;

namespace MugenMvvm.Extensions.Components
{
    public static class SerializationComponentExtension
    {
        #region Methods

        public static void OnSerializing(this ISerializerListener[] listeners, ISerializer serializer, object? instance, ISerializationContext serializationContext)
        {
            Should.NotBeNull(listeners, nameof(listeners));
            Should.NotBeNull(serializer, nameof(serializer));
            Should.NotBeNull(serializationContext, nameof(serializationContext));
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnSerializing(serializer, instance, serializationContext!);
        }

        public static void OnSerialized(this ISerializerListener[] listeners, ISerializer serializer, object? instance, ISerializationContext serializationContext)
        {
            Should.NotBeNull(listeners, nameof(listeners));
            Should.NotBeNull(serializer, nameof(serializer));
            Should.NotBeNull(serializationContext, nameof(serializationContext));
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnSerialized(serializer, instance, serializationContext!);
        }

        public static void OnDeserializing(this ISerializerListener[] listeners, ISerializer serializer, object? instance, ISerializationContext serializationContext)
        {
            Should.NotBeNull(listeners, nameof(listeners));
            Should.NotBeNull(serializer, nameof(serializer));
            Should.NotBeNull(serializationContext, nameof(serializationContext));
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnDeserializing(serializer, instance, serializationContext!);
        }

        public static void OnDeserialized(this ISerializerListener[] listeners, ISerializer serializer, object? instance, ISerializationContext serializationContext)
        {
            Should.NotBeNull(listeners, nameof(listeners));
            Should.NotBeNull(serializer, nameof(serializer));
            Should.NotBeNull(serializationContext, nameof(serializationContext));
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnDeserialized(serializer, instance, serializationContext!);
        }

        public static ISurrogateProvider? TryGetSurrogateProvider(this ISurrogateProviderComponent[] components, ISerializer serializer, Type type, ISerializationContext? serializationContext)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(serializer, nameof(serializer));
            Should.NotBeNull(type, nameof(type));
            for (var i = 0; i < components.Length; i++)
            {
                var provider = components[i].TryGetSurrogateProvider(serializer, type, serializationContext);
                if (provider != null)
                    return provider;
            }

            return null;
        }

        public static Type? TryResolveType(this ITypeResolverComponent[] components, ISerializer serializer, string assemblyName, string typeName, ISerializationContext? serializationContext)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(serializer, nameof(serializer));
            Should.NotBeNull(assemblyName, nameof(assemblyName));
            Should.NotBeNull(typeName, nameof(typeName));
            for (var i = 0; i < components.Length; i++)
            {
                var type = components[i].TryResolveType(serializer, assemblyName, typeName, serializationContext);
                if (type != null)
                    return type;
            }

            return null;
        }

        public static bool TryResolveName(this ITypeResolverComponent[] components, ISerializer serializer, Type serializedType, ISerializationContext? serializationContext, out string? assemblyName, out string? typeName)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(serializer, nameof(serializer));
            Should.NotBeNull(serializedType, nameof(serializedType));
            for (var i = 0; i < components.Length; i++)
            {
                if (components[i].TryResolveName(serializer, serializedType, serializationContext, out assemblyName, out typeName))
                    return true;
            }

            assemblyName = null;
            typeName = null;
            return false;
        }

        public static ISerializationContext? TryGetSerializationContext<TRequest>(this ISerializationContextProviderComponent[] components, ISerializer serializer, [DisallowNull] in TRequest request,
            IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(serializer, nameof(serializer));
            for (var i = 0; i < components.Length; i++)
            {
                var context = components[i].TryGetSerializationContext(serializer, request, metadata);
                if (context != null)
                    return context;
            }

            return null;
        }

        public static ISerializationContext? TryGetDeserializationContext(this ISerializationContextProviderComponent[] components, ISerializer serializer, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(serializer, nameof(serializer));
            for (var i = 0; i < components.Length; i++)
            {
                var context = components[i].TryGetDeserializationContext(serializer, metadata);
                if (context != null)
                    return context;
            }

            return null;
        }

        public static bool TrySerialize<TRequest>(this ISerializerComponent[] components, ISerializer serializer, Stream stream, [DisallowNull] in TRequest request, ISerializationContext serializationContext)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(serializer, nameof(serializer));
            Should.NotBeNull(serializationContext, nameof(serializationContext));
            Should.NotBeNull(stream, nameof(stream));
            for (var i = 0; i < components.Length; i++)
            {
                if (components[i].TrySerialize(serializer, stream, request, serializationContext))
                    return true;
            }

            return false;
        }

        public static bool TryDeserialize(this ISerializerComponent[] components, ISerializer serializer, Stream stream, ISerializationContext serializationContext, out object? value)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(serializer, nameof(serializer));
            Should.NotBeNull(stream, nameof(stream));
            for (var i = 0; i < components.Length; i++)
            {
                if (components[i].TryDeserialize(serializer, stream, serializationContext, out value))
                    return true;
            }

            value = null;
            return false;
        }

        #endregion
    }
}