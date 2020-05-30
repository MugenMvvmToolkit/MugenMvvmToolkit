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

        public static bool TryGetSerializationType(this ISurrogateProviderSerializerComponent[] components, Type type, ISerializationContext? serializationContext,
            [NotNullWhen(true)] out ISurrogateProviderSerializerComponent? provider, [NotNullWhen(true)] out Type? surrogateType)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(type, nameof(type));
            for (var i = 0; i < components.Length; i++)
            {
                surrogateType = components[i].TryGetSerializationType(type, serializationContext);
                if (surrogateType != null)
                {
                    provider = components[i];
                    return true;
                }
            }

            provider = null;
            surrogateType = null;
            return false;
        }

        public static Type? TryResolveType(this ITypeResolverSerializerComponent[] components, string assemblyName, string typeName, ISerializationContext? serializationContext)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(assemblyName, nameof(assemblyName));
            Should.NotBeNull(typeName, nameof(typeName));
            for (var i = 0; i < components.Length; i++)
            {
                var type = components[i].TryResolveType(assemblyName, typeName, serializationContext);
                if (type != null)
                    return type;
            }

            return null;
        }

        public static bool TryResolveName(this ITypeResolverSerializerComponent[] components, Type serializedType, ISerializationContext? serializationContext, out string? assemblyName, out string? typeName)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(serializedType, nameof(serializedType));
            for (var i = 0; i < components.Length; i++)
            {
                if (components[i].TryResolveName(serializedType, serializationContext, out assemblyName, out typeName))
                    return true;
            }

            assemblyName = null;
            typeName = null;
            return false;
        }

        public static ISerializationContext? TryGetSerializationContext(this ISerializationContextProviderComponent[] components, ISerializer serializer, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(serializer, nameof(serializer));
            for (var i = 0; i < components.Length; i++)
            {
                var context = components[i].TryGetSerializationContext(serializer, metadata);
                if (context != null)
                    return context;
            }

            return null;
        }

        public static bool CanSerialize<TRequest>(this ISerializerComponent[] components, [DisallowNull]in TRequest request, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            for (var i = 0; i < components.Length; i++)
            {
                if (components[i].CanSerialize(request, metadata))
                    return true;
            }

            return false;
        }

        public static Stream? TrySerialize<TRequest>(this ISerializerComponent[] components, [DisallowNull]in TRequest request, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            for (var i = 0; i < components.Length; i++)
            {
                var result = components[i].TrySerialize(request, metadata);
                if (result != null)
                    return result;
            }

            return null;
        }

        public static object? TryDeserialize(this ISerializerComponent[] components, Stream stream, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(stream, nameof(stream));
            for (var i = 0; i < components.Length; i++)
            {
                var result = components[i].TryDeserialize(stream, metadata);
                if (result != null)
                    return result;
            }

            return null;
        }

        #endregion
    }
}