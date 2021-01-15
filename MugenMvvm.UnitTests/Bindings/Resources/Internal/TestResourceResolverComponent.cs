using System;
using MugenMvvm.Bindings.Interfaces.Resources;
using MugenMvvm.Bindings.Interfaces.Resources.Components;
using MugenMvvm.Bindings.Resources;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using Should;

namespace MugenMvvm.UnitTests.Bindings.Resources.Internal
{
    public class TestResourceResolverComponent : IResourceResolverComponent, IHasPriority
    {
        private readonly IResourceManager? _resourceResolver;

        public TestResourceResolverComponent(IResourceManager? resourceResolver = null)
        {
            _resourceResolver = resourceResolver;
        }

        public Func<string, object?, IReadOnlyMetadataContext?, ResourceResolverResult>? TryGetResource { get; set; }

        public int Priority { get; set; }

        ResourceResolverResult IResourceResolverComponent.TryGetResource(IResourceManager resourceManager, string name, object? state, IReadOnlyMetadataContext? metadata)
        {
            _resourceResolver?.ShouldEqual(resourceManager);
            return TryGetResource?.Invoke(name, state, metadata) ?? default;
        }
    }
}