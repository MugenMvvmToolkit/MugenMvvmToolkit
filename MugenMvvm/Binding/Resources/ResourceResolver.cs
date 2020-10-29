using System;
using MugenMvvm.Bindings.Extensions.Components;
using MugenMvvm.Bindings.Interfaces.Resources;
using MugenMvvm.Bindings.Interfaces.Resources.Components;
using MugenMvvm.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Bindings.Resources
{
    public sealed class ResourceResolver : ComponentOwnerBase<IResourceResolver>, IResourceResolver
    {
        #region Constructors

        public ResourceResolver(IComponentCollectionManager? componentCollectionManager = null) : base(componentCollectionManager)
        {
        }

        #endregion

        #region Implementation of interfaces

        public ResourceResolverResult TryGetResource(string name, object? state = null, IReadOnlyMetadataContext? metadata = null) =>
            GetComponents<IResourceResolverComponent>(metadata).TryGetResource(this, name, state, metadata);

        public Type? TryGetType(string name, object? state = null, IReadOnlyMetadataContext? metadata = null) => GetComponents<ITypeResolverComponent>(metadata).TryGetType(this, name, state, metadata);

        #endregion
    }
}