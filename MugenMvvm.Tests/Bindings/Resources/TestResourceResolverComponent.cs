using System;
using MugenMvvm.Bindings.Interfaces.Resources;
using MugenMvvm.Bindings.Interfaces.Resources.Components;
using MugenMvvm.Bindings.Resources;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Tests.Bindings.Resources
{
    public class TestResourceResolverComponent : IResourceResolverComponent, IHasPriority
    {
        public Func<IResourceManager, string, object?, IReadOnlyMetadataContext?, ResourceResolverResult>? TryGetResource { get; set; }

        public int Priority { get; set; }

        ResourceResolverResult IResourceResolverComponent.TryGetResource(IResourceManager resourceManager, string name, object? state, IReadOnlyMetadataContext? metadata) =>
            TryGetResource?.Invoke(resourceManager, name, state, metadata) ?? default;
    }
}