using System;
using MugenMvvm.Binding.Interfaces.Resources;
using MugenMvvm.Binding.Interfaces.Resources.Components;
using MugenMvvm.Binding.Resources;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using Should;

namespace MugenMvvm.UnitTests.Binding.Resources.Internal
{
    public class TestResourceResolverComponent : IResourceResolverComponent, IHasPriority
    {
        #region Fields

        private readonly IResourceResolver? _resourceResolver;

        #endregion

        #region Constructors

        public TestResourceResolverComponent(IResourceResolver? resourceResolver = null)
        {
            _resourceResolver = resourceResolver;
        }

        #endregion

        #region Properties

        public int Priority { get; set; }

        public Func<string, object?, IReadOnlyMetadataContext?, ResourceResolverResult>? TryGetResource { get; set; }

        #endregion

        #region Implementation of interfaces

        ResourceResolverResult IResourceResolverComponent.TryGetResource(IResourceResolver resourceResolver, string name, object? state, IReadOnlyMetadataContext? metadata)
        {
            _resourceResolver?.ShouldEqual(resourceResolver);
            return TryGetResource?.Invoke(name, state, metadata) ?? default;
        }

        #endregion
    }
}