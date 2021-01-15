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
        private readonly IResourceManager? _resourceResolver;

        public TestTypeResolverComponent(IResourceManager? resourceResolver = null)
        {
            _resourceResolver = resourceResolver;
        }

        public Func<string, object?, IReadOnlyMetadataContext?, Type?>? TryGetType { get; set; }

        public int Priority { get; set; }

        Type? ITypeResolverComponent.TryGetType(IResourceManager resourceManager, string name, object? state, IReadOnlyMetadataContext? metadata)
        {
            _resourceResolver?.ShouldEqual(resourceManager);
            return TryGetType?.Invoke(name, state, metadata);
        }
    }
}