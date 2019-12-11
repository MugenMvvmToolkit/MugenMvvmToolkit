using System;
using MugenMvvm.Binding.Extensions.Components;
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
            return GetComponents<IResourceResolverComponent>(metadata).TryGetResourceValue(name, metadata);
        }

        public IBindingValueConverter? TryGetConverter(string name, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(name, nameof(name));
            return GetComponents<IBindingValueConverterResolverComponent>(metadata).TryGetConverter(name, metadata);
        }

        public Type? TryGetType(string name, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(name, nameof(name));
            return GetComponents<ITypeResolverComponent>(metadata).TryGetType(name, metadata);
        }

        #endregion
    }
}