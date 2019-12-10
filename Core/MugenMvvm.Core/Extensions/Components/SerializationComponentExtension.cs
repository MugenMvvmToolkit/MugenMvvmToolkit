using System;
using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Serialization;
using MugenMvvm.Interfaces.Serialization.Components;

namespace MugenMvvm.Extensions.Components
{
    public static class SerializationComponentExtension
    {
        #region Methods

        public static void OnContextCreated(this ISerializerListener[] listeners, ISerializer serializer, ISerializationContext serializationContext)
        {
            Should.NotBeNull(listeners, nameof(listeners));
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnContextCreated(serializer, serializationContext);
        }

        public static void OnSerializing(this ISerializerListener[] listeners, ISerializer serializer, object? instance, ISerializationContext serializationContext)
        {
            Should.NotBeNull(listeners, nameof(listeners));
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnSerializing(serializer, instance, serializationContext!);
        }

        public static void OnSerialized(this ISerializerListener[] listeners, ISerializer serializer, object? instance, ISerializationContext serializationContext)
        {
            Should.NotBeNull(listeners, nameof(listeners));
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnSerialized(serializer, instance, serializationContext!);
        }

        public static void OnDeserializing(this ISerializerListener[] listeners, ISerializer serializer, object? instance, ISerializationContext serializationContext)
        {
            Should.NotBeNull(listeners, nameof(listeners));
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnDeserializing(serializer, instance, serializationContext!);
        }

        public static void OnDeserialized(this ISerializerListener[] listeners, ISerializer serializer, object? instance, ISerializationContext serializationContext)
        {
            Should.NotBeNull(listeners, nameof(listeners));
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnDeserialized(serializer, instance, serializationContext!);
        }

        public static bool TryGetSerializationType(this ISurrogateProviderSerializerComponent[] components, Type type, ISerializationContext? serializationContext,
            [NotNullWhen(true)] out ISurrogateProviderSerializerComponent? provider, [NotNullWhen(true)] out Type? surrogateType)
        {
            Should.NotBeNull(components, nameof(components));
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
            for (var i = 0; i < components.Length; i++)
            {
                var context = components[i].TryGetSerializationContext(serializer, metadata);
                if (context != null)
                    return context;
            }

            return null;
        }

        #endregion
    }
}