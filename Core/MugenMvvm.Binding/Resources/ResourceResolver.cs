using System;
using MugenMvvm.Binding.Interfaces.Converters;
using MugenMvvm.Binding.Interfaces.Resources;
using MugenMvvm.Binding.Interfaces.Resources.Components;
using MugenMvvm.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Resources
{
    public sealed class ResourceResolver : ComponentOwnerBase<IResourceResolver>, IResourceResolver, 
        IComponentOwnerAddedCallback<IComponent<IResourceResolver>>, IComponentOwnerRemovedCallback<IComponent<IResourceResolver>>
    {
        #region Fields

        private IBindingValueConverterResolverComponent[] _converterResolvers;
        private IResourceResolverComponent[] _resourceResolvers;
        private ITypeResolverComponent[] _typeResolvers;

        #endregion

        #region Constructors

        public ResourceResolver(IComponentCollectionProvider? componentCollectionProvider = null) : base(componentCollectionProvider)
        {
            _typeResolvers = Default.EmptyArray<ITypeResolverComponent>();
            _resourceResolvers = Default.EmptyArray<IResourceResolverComponent>();
            _converterResolvers = Default.EmptyArray<IBindingValueConverterResolverComponent>();
        }

        #endregion

        #region Implementation of interfaces

        void IComponentOwnerAddedCallback<IComponent<IResourceResolver>>.OnComponentAdded(IComponentCollection<IComponent<IResourceResolver>> collection,
            IComponent<IResourceResolver> component, IReadOnlyMetadataContext? metadata)
        {
            MugenExtensions.ComponentTrackerOnAdded(ref _converterResolvers, collection, component);
            MugenExtensions.ComponentTrackerOnAdded(ref _resourceResolvers, collection, component);
            MugenExtensions.ComponentTrackerOnAdded(ref _typeResolvers, collection, component);
        }

        void IComponentOwnerRemovedCallback<IComponent<IResourceResolver>>.OnComponentRemoved(IComponentCollection<IComponent<IResourceResolver>> collection,
            IComponent<IResourceResolver> component, IReadOnlyMetadataContext? metadata)
        {
            MugenExtensions.ComponentTrackerOnRemoved(ref _converterResolvers, component);
            MugenExtensions.ComponentTrackerOnRemoved(ref _resourceResolvers, component);
            MugenExtensions.ComponentTrackerOnRemoved(ref _typeResolvers, component);
        }

        public IResourceValue? TryGetResourceValue(string name, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(name, nameof(name));
            var resolvers = _resourceResolvers;
            for (var i = 0; i < resolvers.Length; i++)
            {
                var value = resolvers[i].TryGetResourceValue(name, metadata);
                if (value != null)
                    return value;
            }

            return null;
        }

        public IBindingValueConverter? TryGetConverter(string name, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(name, nameof(name));
            var resolvers = _converterResolvers;
            for (var i = 0; i < resolvers.Length; i++)
            {
                var converter = resolvers[i].TryGetConverter(name, metadata);
                if (converter != null)
                    return converter;
            }

            return null;
        }

        public Type? TryGetType(string name, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(name, nameof(name));
            var resolvers = _typeResolvers;
            for (var i = 0; i < resolvers.Length; i++)
            {
                var type = resolvers[i].TryGetType(name, metadata);
                if (type != null)
                    return type;
            }

            return null;
        }

        #endregion
    }
}