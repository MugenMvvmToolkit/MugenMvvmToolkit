using System;
using MugenMvvm.Binding.Interfaces.Resources;
using MugenMvvm.Binding.Interfaces.Resources.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using Should;

namespace MugenMvvm.UnitTest.Binding.Resources.Internal
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

        public Func<string, object?, Type, IReadOnlyMetadataContext?, IResourceValue?>? TryGetResourceValue { get; set; }

        #endregion

        #region Implementation of interfaces

        IResourceValue? IResourceResolverComponent.TryGetResourceValue<TState>(IResourceResolver resourceResolver, string name, in TState state, IReadOnlyMetadataContext? metadata)
        {
            _resourceResolver?.ShouldEqual(resourceResolver);
            return TryGetResourceValue?.Invoke(name, state, typeof(TState), metadata);
        }

        #endregion
    }
}