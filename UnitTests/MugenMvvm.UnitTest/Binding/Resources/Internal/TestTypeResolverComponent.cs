using System;
using MugenMvvm.Binding.Interfaces.Resources;
using MugenMvvm.Binding.Interfaces.Resources.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using Should;

namespace MugenMvvm.UnitTest.Binding.Resources.Internal
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

        public Func<string, object?, Type, IReadOnlyMetadataContext?, Type?>? TryGetType { get; set; }

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        Type? ITypeResolverComponent.TryGetType<TRequest>(IResourceResolver resourceResolver, string name, in TRequest state, IReadOnlyMetadataContext? metadata)
        {
            _resourceResolver?.ShouldEqual(resourceResolver);
            return TryGetType?.Invoke(name, state, typeof(TRequest), metadata);
        }

        #endregion
    }
}