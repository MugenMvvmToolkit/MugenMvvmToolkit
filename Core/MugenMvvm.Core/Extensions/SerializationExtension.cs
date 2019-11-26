using System;
using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Serialization;
using MugenMvvm.Interfaces.Serialization.Components;

// ReSharper disable once CheckNamespace
namespace MugenMvvm
{
    public static partial class MugenExtensions
    {
        #region Methods

        public static void OnContextCreated(this ISerializer serializer, ISerializationContext serializationContext)
        {
            Should.NotBeNull(serializer, nameof(serializer));
            Should.NotBeNull(serializationContext, nameof(serializationContext));
            var components = serializer.GetComponents();
            for (var i = 0; i < components.Length; i++)
                (components[i] as ISerializerListener)?.OnContextCreated(serializer, serializationContext);
        }

        public static void OnSerializing(this ISerializer serializer, object? instance, ISerializationContext serializationContext)
        {
            Should.NotBeNull(serializer, nameof(serializer));
            Should.NotBeNull(serializationContext, nameof(serializationContext));
            var components = serializer.GetComponents();
            for (var i = 0; i < components.Length; i++)
                (components[i] as ISerializerListener)?.OnSerializing(serializer, instance, serializationContext!);
        }

        public static void OnSerialized(this ISerializer serializer, object? instance, ISerializationContext serializationContext)
        {
            Should.NotBeNull(serializer, nameof(serializer));
            Should.NotBeNull(serializationContext, nameof(serializationContext));
            var components = serializer.GetComponents();
            for (var i = 0; i < components.Length; i++)
                (components[i] as ISerializerListener)?.OnSerialized(serializer, instance, serializationContext!);
        }

        public static void OnDeserializing(this ISerializer serializer, object? instance, ISerializationContext serializationContext)
        {
            Should.NotBeNull(serializer, nameof(serializer));
            Should.NotBeNull(serializationContext, nameof(serializationContext));
            var components = serializer.GetComponents();
            for (var i = 0; i < components.Length; i++)
                (components[i] as ISerializerListener)?.OnDeserializing(serializer, instance, serializationContext!);
        }

        public static void OnDeserialized(this ISerializer serializer, object? instance, ISerializationContext serializationContext)
        {
            Should.NotBeNull(serializer, nameof(serializer));
            Should.NotBeNull(serializationContext, nameof(serializationContext));
            var components = serializer.GetComponents();
            for (var i = 0; i < components.Length; i++)
                (components[i] as ISerializerListener)?.OnDeserialized(serializer, instance, serializationContext!);
        }

        public static bool TryGetSurrogateSerializer(this ISerializer serializer, Type type, ISerializationContext? serializationContext,
            [NotNullWhen(true)] out ISurrogateProviderSerializerComponent? provider, [NotNullWhen(true)] out Type? surrogateType)
        {
            Should.NotBeNull(serializer, nameof(serializer));
            var components = serializer.GetComponents();
            for (var i = 0; i < components.Length; i++)
            {
                if (!(components[i] is ISurrogateProviderSerializerComponent surrogate))
                    continue;

                surrogateType = surrogate.TryGetSerializationType(type, serializationContext);
                if (surrogateType != null)
                {
                    provider = surrogate;
                    return true;
                }
            }

            provider = null;
            surrogateType = null;
            return false;
        }

        public static Type? TryResolveType(this ISerializer serializer, string assemblyName, string typeName, ISerializationContext? serializationContext)
        {
            Should.NotBeNull(serializer, nameof(serializer));
            var components = serializer.GetComponents();
            for (var i = 0; i < components.Length; i++)
            {
                var type = (components[i] as ITypeResolverSerializerComponent)?.TryResolveType(assemblyName, typeName, serializationContext);
                if (type != null)
                    return type;
            }

            return null;
        }

        public static bool TryResolveName(this ISerializer serializer, Type serializedType, ISerializationContext? serializationContext, out string? assemblyName, out string? typeName)
        {
            Should.NotBeNull(serializer, nameof(serializer));
            var components = serializer.GetComponents();
            for (var i = 0; i < components.Length; i++)
            {
                if (components[i] is ITypeResolverSerializerComponent resolver && resolver.TryResolveName(serializedType, serializationContext, out assemblyName, out typeName))
                    return true;
            }

            assemblyName = null;
            typeName = null;
            return false;
        }

        public static ISerializationContext GetSerializationContext(this ISerializer serializer, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(serializer, nameof(serializer));
            var components = serializer.GetComponents();
            for (var i = 0; i < components.Length; i++)
            {
                var context = (components[i] as ISerializationContextProviderComponent)?.TryGetSerializationContext(metadata);
                if (context != null)
                    return context;
            }

            ExceptionManager.ThrowObjectNotInitialized(serializer, typeof(ISerializationContextProviderComponent).Name);
            return null;
        }

        #endregion
    }
}