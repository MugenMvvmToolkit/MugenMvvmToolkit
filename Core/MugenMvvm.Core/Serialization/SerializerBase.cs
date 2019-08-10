using System;
using System.IO;
using System.Runtime.CompilerServices;
using MugenMvvm.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Serialization;
using MugenMvvm.Interfaces.Serialization.Components;

namespace MugenMvvm.Serialization
{
    public abstract class SerializerBase : ComponentOwnerBase<ISerializer>, ISerializer, IComponentOwnerAddedCallback<IComponent<ISerializer>>,
        IComponentOwnerRemovedCallback<IComponent<ISerializer>>
    {
        #region Fields

        private readonly IMetadataContextProvider? _metadataContextProvider;

        #endregion

        #region Constructors

        protected SerializerBase(IComponentCollectionProvider? componentCollectionProvider = null, IMetadataContextProvider? metadataContextProvider = null)
            : base(componentCollectionProvider)
        {
            _metadataContextProvider = metadataContextProvider;
        }

        #endregion

        #region Properties

        protected IMetadataContextProvider MetadataContextProvider => _metadataContextProvider.ServiceIfNull();

        public abstract bool IsOnSerializingSupported { get; }

        public abstract bool IsOnSerializedSupported { get; }

        public abstract bool IsOnDeserializingSupported { get; }

        public abstract bool IsOnDeserializedSupported { get; }

        [field: ThreadStatic]
        public static ISerializationContext? CurrentSerializationContext { get; private set; }

        #endregion

        #region Implementation of interfaces

        void IComponentOwnerAddedCallback<IComponent<ISerializer>>.OnComponentAdded(object collection, IComponent<ISerializer> component, IReadOnlyMetadataContext? metadata)
        {
            OnComponentAdded(component, metadata);
        }

        void IComponentOwnerRemovedCallback<IComponent<ISerializer>>.OnComponentRemoved(object collection, IComponent<ISerializer> component, IReadOnlyMetadataContext? metadata)
        {
            OnComponentRemoved(component, metadata);
        }

        public ISerializationContext GetSerializationContext(IServiceProvider? serviceProvider, IReadOnlyMetadataContext? metadata)
        {
            var context = GetSerializationContextInternal(serviceProvider, metadata);
            OnContextCreated(context);
            return context;
        }

        public bool CanSerialize(Type type, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(type, nameof(type));
            return CanSerializeInternal(type, metadata);
        }

        public Stream Serialize(object item, ISerializationContext? serializationContext = null)
        {
            Should.NotBeNull(item, nameof(item));
            if (serializationContext == null)
                serializationContext = GetSerializationContext(null, null);
            try
            {
                CurrentSerializationContext = serializationContext;
                return SerializeInternal(item);
            }
            finally
            {
                CurrentSerializationContext = null;
            }
        }

        public object Deserialize(Stream stream, ISerializationContext? serializationContext = null)
        {
            Should.NotBeNull(stream, nameof(stream));
            if (serializationContext == null)
                serializationContext = GetSerializationContext(null, null);
            try
            {
                CurrentSerializationContext = serializationContext;
                return DeserializeInternal(stream);
            }
            finally
            {
                CurrentSerializationContext = null;
            }
        }

        #endregion

        #region Methods

        protected abstract Stream SerializeInternal(object item);

        protected abstract object DeserializeInternal(Stream stream);

        protected virtual bool CanSerializeInternal(Type type, IReadOnlyMetadataContext? metadata)
        {
            return type.IsSerializableUnified() || TryGetSurrogateSerializerHandler(type, out _, out _);
        }

        protected virtual void OnContextCreated(ISerializationContext context)
        {
            var components = GetComponents();
            for (var i = 0; i < components.Length; i++)
                (components[i] as ISerializerListener)?.OnContextCreated(this, context);
        }

        protected virtual void OnSerializing(object? instance)
        {
            var components = GetComponents();
            for (var i = 0; i < components.Length; i++)
                (components[i] as ISerializerListener)?.OnSerializing(this, instance, CurrentSerializationContext!);
        }

        protected virtual void OnSerialized(object? instance)
        {
            var components = GetComponents();
            for (var i = 0; i < components.Length; i++)
                (components[i] as ISerializerListener)?.OnSerialized(this, instance, CurrentSerializationContext!);
        }

        protected virtual void OnDeserializing(object? instance)
        {
            var components = GetComponents();
            for (var i = 0; i < components.Length; i++)
                (components[i] as ISerializerListener)?.OnDeserializing(this, instance, CurrentSerializationContext!);
        }

        protected virtual void OnDeserialized(object? instance)
        {
            var components = GetComponents();
            for (var i = 0; i < components.Length; i++)
                (components[i] as ISerializerListener)?.OnDeserialized(this, instance, CurrentSerializationContext!);
        }

        protected virtual bool TryGetSurrogateSerializerHandler(Type type, [NotNullWhenTrue] out ISurrogateProviderComponent? provider,
            [NotNullWhenTrue] out Type? surrogateType)
        {
            var components = GetComponents();
            for (var i = 0; i < components.Length; i++)
            {
                if (components[i] is ISurrogateProviderComponent surrogate)
                {
                    surrogateType = surrogate.TryGetSerializationType(this, type);
                    if (surrogateType != null)
                    {
                        provider = surrogate;
                        return true;
                    }
                }
            }

            provider = null;
            surrogateType = null;
            return false;
        }

        protected virtual Type? TryResolveType(string assemblyName, string typeName)
        {
            var components = GetComponents();
            for (var i = 0; i < components.Length; i++)
            {
                if (components[i] is ITypeResolverComponent resolver)
                {
                    var type = resolver.TryResolveType(this, assemblyName, typeName);
                    if (type != null)
                        return type;
                }
            }

            return null;
        }

        protected virtual bool TryResolveName(Type serializedType, out string? assemblyName, out string? typeName)
        {
            var components = GetComponents();
            for (var i = 0; i < components.Length; i++)
            {
                if (components[i] is ITypeResolverComponent resolver && resolver.TryResolveName(this, serializedType, out assemblyName, out typeName))
                    return true;
            }

            assemblyName = null;
            typeName = null;
            return false;
        }

        protected virtual ISerializationContext GetSerializationContextInternal(IServiceProvider? serviceProvider, IReadOnlyMetadataContext? metadata)
        {
            var components = GetComponents();
            for (var i = 0; i < components.Length; i++)
            {
                var context = (components[i] as ISerializationContextProviderComponent)?.TryGetSerializationContext(this, serviceProvider, metadata);
                if (context != null)
                    return context;
            }

            return new SerializationContext(this, metadata.ToNonReadonly(this, MetadataContextProvider));//todo lazy context
        }

        protected virtual void OnComponentAdded(IComponent<ISerializer> component, IReadOnlyMetadataContext? metadata)
        {
        }

        protected virtual void OnComponentRemoved(IComponent<ISerializer> component, IReadOnlyMetadataContext? metadata)
        {
        }

        #endregion
    }
}