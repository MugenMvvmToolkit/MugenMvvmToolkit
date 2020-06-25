using System;
using MugenMvvm.Binding.Extensions.Components;
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

        public IResourceValue? TryGetResourceValue<TState>(string name, in TState state, IReadOnlyMetadataContext? metadata = null)
        {
            return GetComponents<IResourceResolverComponent>(metadata).TryGetResourceValue(name, state, metadata);
        }

        public Type? TryGetType<TState>(string name, in TState state, IReadOnlyMetadataContext? metadata = null)
        {
            return GetComponents<ITypeResolverComponent>(metadata).TryGetType(name, state, metadata);
        }

        #endregion
    }
}