using System;
using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Collections;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Serialization;
using MugenMvvm.Interfaces.Serialization.Components;

namespace MugenMvvm.Extensions.Components
{
    public static class SerializationComponentExtension
    {
        public static void OnSerializing(this ItemOrArray<ISerializerListener> listeners, ISerializer serializer, object? instance, ISerializationContext serializationContext)
        {
            Should.NotBeNull(serializer, nameof(serializer));
            Should.NotBeNull(serializationContext, nameof(serializationContext));
            foreach (var c in listeners)
                c.OnSerializing(serializer, instance, serializationContext!);
        }

        public static void OnSerialized(this ItemOrArray<ISerializerListener> listeners, ISerializer serializer, object? instance, ISerializationContext serializationContext)
        {
            Should.NotBeNull(serializer, nameof(serializer));
            Should.NotBeNull(serializationContext, nameof(serializationContext));
            foreach (var c in listeners)
                c.OnSerialized(serializer, instance, serializationContext!);
        }

        public static void OnDeserializing(this ItemOrArray<ISerializerListener> listeners, ISerializer serializer, object? instance, ISerializationContext serializationContext)
        {
            Should.NotBeNull(serializer, nameof(serializer));
            Should.NotBeNull(serializationContext, nameof(serializationContext));
            foreach (var c in listeners)
                c.OnDeserializing(serializer, instance, serializationContext!);
        }

        public static void OnDeserialized(this ItemOrArray<ISerializerListener> listeners, ISerializer serializer, object? instance, ISerializationContext serializationContext)
        {
            Should.NotBeNull(serializer, nameof(serializer));
            Should.NotBeNull(serializationContext, nameof(serializationContext));
            foreach (var c in listeners)
                c.OnDeserialized(serializer, instance, serializationContext!);
        }

        public static ISurrogateProvider? TryGetSurrogateProvider(this ItemOrArray<ISurrogateProviderResolverComponent> components, ISerializer serializer, Type type,
            ISerializationContext? serializationContext)
        {
            Should.NotBeNull(serializer, nameof(serializer));
            Should.NotBeNull(type, nameof(type));
            foreach (var c in components)
            {
                var provider = c.TryGetSurrogateProvider(serializer, type, serializationContext);
                if (provider != null)
                    return provider;
            }

            return null;
        }

        public static Type? TryResolveType(this ItemOrArray<ITypeResolverComponent> components, ISerializer serializer, string? assemblyName, string typeName,
            ISerializationContext? serializationContext)
        {
            Should.NotBeNull(serializer, nameof(serializer));
            Should.NotBeNull(typeName, nameof(typeName));
            foreach (var c in components)
            {
                var type = c.TryResolveType(serializer, assemblyName, typeName, serializationContext);
                if (type != null)
                    return type;
            }

            return null;
        }

        public static bool TryResolveName(this ItemOrArray<ITypeResolverComponent> components, ISerializer serializer, Type serializedType,
            ISerializationContext? serializationContext, out string? assemblyName,
            out string? typeName)
        {
            Should.NotBeNull(serializer, nameof(serializer));
            Should.NotBeNull(serializedType, nameof(serializedType));
            foreach (var c in components)
            {
                if (c.TryResolveName(serializer, serializedType, serializationContext, out assemblyName, out typeName))
                    return true;
            }

            assemblyName = null;
            typeName = null;
            return false;
        }

        public static ISerializationContext? TryGetSerializationContext<TRequest, TResult>(this ItemOrArray<ISerializationContextProviderComponent> components,
            ISerializer serializer,
            ISerializationFormatBase<TRequest, TResult> format, TRequest request, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(serializer, nameof(serializer));
            Should.NotBeNull(format, nameof(format));
            foreach (var c in components)
            {
                var context = c.TryGetSerializationContext(serializer, format, request, metadata);
                if (context != null)
                    return context;
            }

            return null;
        }

        public static bool IsSupported<TRequest, TResult>(this ItemOrArray<ISerializationManagerComponent> components, ISerializer serializer,
            ISerializationFormatBase<TRequest, TResult> format, TRequest? request,
            IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(serializer, nameof(serializer));
            Should.NotBeNull(format, nameof(format));
            foreach (var c in components)
            {
                if (c.IsSupported(serializer, format, request, metadata))
                    return true;
            }

            return false;
        }

        public static bool TrySerialize<TRequest, TResult>(this ItemOrArray<ISerializationManagerComponent> components, ISerializer serializer,
            ISerializationFormat<TRequest, TResult> format, TRequest request,
            ISerializationContext serializationContext, [NotNullWhen(true)] ref TResult? result)
        {
            Should.NotBeNull(serializer, nameof(serializer));
            Should.NotBeNull(format, nameof(format));
            Should.NotBeNull(serializationContext, nameof(serializationContext));
            foreach (var c in components)
            {
                if (c.TrySerialize(serializer, format, request, serializationContext, ref result))
                    return true;
            }

            result = default!;
            return false;
        }

        public static bool TryDeserialize<TRequest, TResult>(this ItemOrArray<ISerializationManagerComponent> components, ISerializer serializer,
            IDeserializationFormat<TRequest, TResult> format, TRequest request,
            ISerializationContext serializationContext, [NotNullWhen(true)] ref TResult? result)
        {
            Should.NotBeNull(serializer, nameof(serializer));
            Should.NotBeNull(format, nameof(format));
            Should.NotBeNull(serializationContext, nameof(serializationContext));
            foreach (var c in components)
            {
                if (c.TryDeserialize(serializer, format, request, serializationContext, ref result))
                    return true;
            }

            result = default!;
            return false;
        }

        public static bool IsSupported<TRequest, TResult>(this ItemOrArray<ISerializerComponent<TRequest, TResult>> components, ISerializer serializer,
            ISerializationFormat<TRequest, TResult> format,
            TRequest? request, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(serializer, nameof(serializer));
            Should.NotBeNull(format, nameof(format));
            foreach (var c in components)
            {
                if (c.IsSupported(serializer, format, request, metadata))
                    return true;
            }

            return false;
        }

        public static bool TrySerialize<TRequest, TResult>(this ItemOrArray<ISerializerComponent<TRequest, TResult>> components, ISerializer serializer,
            ISerializationFormat<TRequest, TResult> format, TRequest request,
            ISerializationContext serializationContext, [NotNullWhen(true)] ref TResult? result)
        {
            Should.NotBeNull(serializer, nameof(serializer));
            Should.NotBeNull(format, nameof(format));
            Should.NotBeNull(serializationContext, nameof(serializationContext));
            foreach (var c in components)
            {
                if (c.TrySerialize(serializer, format, request, serializationContext, ref result))
                    return true;
            }

            result = default!;
            return false;
        }

        public static bool IsSupported<TRequest, TResult>(this ItemOrArray<IDeserializerComponent<TRequest, TResult>> components, ISerializer serializer,
            IDeserializationFormat<TRequest, TResult> format,
            TRequest? request, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(serializer, nameof(serializer));
            Should.NotBeNull(format, nameof(format));
            foreach (var c in components)
            {
                if (c.IsSupported(serializer, format, request, metadata))
                    return true;
            }

            return false;
        }

        public static bool TryDeserialize<TRequest, TResult>(this ItemOrArray<IDeserializerComponent<TRequest, TResult>> components, ISerializer serializer,
            IDeserializationFormat<TRequest, TResult> format, TRequest request,
            ISerializationContext serializationContext, [NotNullWhen(true)] ref TResult? result)
        {
            Should.NotBeNull(serializer, nameof(serializer));
            Should.NotBeNull(format, nameof(format));
            Should.NotBeNull(serializationContext, nameof(serializationContext));
            foreach (var c in components)
            {
                if (c.TryDeserialize(serializer, format, request, serializationContext, ref result))
                    return true;
            }

            result = default!;
            return false;
        }
    }
}