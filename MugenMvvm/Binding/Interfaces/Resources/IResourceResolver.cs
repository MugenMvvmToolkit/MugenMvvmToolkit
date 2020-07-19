using System;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Resources
{
    public interface IResourceResolver : IComponentOwner<IResourceResolver>
    {
        IResourceValue? TryGetResourceValue(string name, object? state = null, IReadOnlyMetadataContext? metadata = null);

        Type? TryGetType(string name, object? state = null, IReadOnlyMetadataContext? metadata = null);
    }
}