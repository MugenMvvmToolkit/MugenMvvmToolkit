using System;
using MugenMvvm.Binding.Resources;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Resources
{
    public interface IResourceResolver : IComponentOwner<IResourceResolver>
    {
        ResourceResolverResult TryGetResource(string name, object? state = null, IReadOnlyMetadataContext? metadata = null);

        Type? TryGetType(string name, object? state = null, IReadOnlyMetadataContext? metadata = null);
    }
}