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

        public IResourceValue? TryGetResourceValue<TRequest>(string name, in TRequest request, IReadOnlyMetadataContext? metadata = null)
        {
            return GetComponents<IResourceResolverComponent>(metadata).TryGetResourceValue(name, request, metadata);
        }

        public IBindingValueConverter? TryGetConverter<TRequest>(string name, in TRequest request, IReadOnlyMetadataContext? metadata = null)
        {
            return GetComponents<IBindingValueConverterResolverComponent>(metadata).TryGetConverter(name, request, metadata);
        }

        public Type? TryGetType<TRequest>(string name, in TRequest request, IReadOnlyMetadataContext? metadata = null)
        {
            return GetComponents<ITypeResolverComponent>(metadata).TryGetType(name, request, metadata);
        }

        #endregion
    }
}