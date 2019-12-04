using System;
using MugenMvvm.Binding.Interfaces.Converters;
using MugenMvvm.Binding.Interfaces.Resources;
using MugenMvvm.Binding.Interfaces.Resources.Components;
using MugenMvvm.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Resources
{
    public sealed class ResourceResolver : ComponentOwnerBase<IResourceResolver>, IResourceResolver
    {
        #region Constructors

        public ResourceResolver(IComponentCollectionProvider? componentCollectionProvider = null) : base(componentCollectionProvider)
        {
        }

        #endregion

        #region Implementation of interfaces

        public IResourceValue? TryGetResourceValue(string name, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(name, nameof(name));
            var resolvers = GetComponents<IResourceResolverComponent>(metadata);
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
            var resolvers = GetComponents<IBindingValueConverterResolverComponent>(metadata);
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
            var resolvers = GetComponents<ITypeResolverComponent>(metadata);
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