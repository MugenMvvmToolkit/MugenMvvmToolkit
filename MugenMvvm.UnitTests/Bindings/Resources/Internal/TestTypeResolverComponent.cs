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
        #region Fields

        private readonly IResourceResolver? _resourceResolver;

        #endregion

        #region Constructors

        public TestTypeResolverComponent(IResourceResolver? resourceResolver = null)
        {
            _resourceResolver = resourceResolver;
        }

        #endregion

        #region Properties

        public Func<string, object?, IReadOnlyMetadataContext?, Type?>? TryGetType { get; set; }

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        Type? ITypeResolverComponent.TryGetType(IResourceResolver resourceResolver, string name, object? state, IReadOnlyMetadataContext? metadata)
        {
            _resourceResolver?.ShouldEqual(resourceResolver);
            return TryGetType?.Invoke(name, state, metadata);
        }

        #endregion
    }
}