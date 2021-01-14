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
        private readonly IResourceResolver? _resourceResolver;

        public TestResourceResolverComponent(IResourceResolver? resourceResolver = null)
        {
            _resourceResolver = resourceResolver;
        }

        public Func<string, object?, IReadOnlyMetadataContext?, ResourceResolverResult>? TryGetResource { get; set; }

        public int Priority { get; set; }

        ResourceResolverResult IResourceResolverComponent.TryGetResource(IResourceResolver resourceResolver, string name, object? state, IReadOnlyMetadataContext? metadata)
        {
            _resourceResolver?.ShouldEqual(resourceResolver);
            return TryGetResource?.Invoke(name, state, metadata) ?? default;
        }
    }
}