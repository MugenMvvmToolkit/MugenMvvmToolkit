using System;
using MugenMvvm.Bindings.Interfaces.Resources;
using MugenMvvm.Bindings.Interfaces.Resources.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using Should;

namespace MugenMvvm.UnitTests.Bindings.Resources.Internal
{
    public class TestTypeResolverComponent : ITypeResolverComponent, IHasPriority
    {
        private readonly IResourceResolver? _resourceResolver;

        public TestTypeResolverComponent(IResourceResolver? resourceResolver = null)
        {
            _resourceResolver = resourceResolver;
        }

        public Func<string, object?, IReadOnlyMetadataContext?, Type?>? TryGetType { get; set; }

        public int Priority { get; set; }

        Type? ITypeResolverComponent.TryGetType(IResourceResolver resourceResolver, string name, object? state, IReadOnlyMetadataContext? metadata)
        {
            _resourceResolver?.ShouldEqual(resourceResolver);
            return TryGetType?.Invoke(name, state, metadata);
        }
    }
}