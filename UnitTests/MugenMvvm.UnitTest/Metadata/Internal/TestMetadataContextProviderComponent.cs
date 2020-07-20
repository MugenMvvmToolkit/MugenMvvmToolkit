using System;
using System.Collections.Generic;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Metadata.Components;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;
using MugenMvvm.Metadata;
using Should;

namespace MugenMvvm.UnitTest.Metadata.Internal
{
    public class TestMetadataContextProviderComponent : IMetadataContextProviderComponent, IHasPriority
    {
        #region Fields

        private readonly IMetadataContextManager? _metadataContextManager;

        #endregion

        #region Constructors

        public TestMetadataContextProviderComponent(IMetadataContextManager? metadataContextManager = null)
        {
            _metadataContextManager = metadataContextManager;
        }

        #endregion

        #region Properties

        public Func<object?, ItemOrList<KeyValuePair<IMetadataContextKey, object?>, IReadOnlyCollection<KeyValuePair<IMetadataContextKey, object?>>>, IReadOnlyMetadataContext?>? TryGetReadOnlyMetadataContext { get; set; }

        public Func<object?, ItemOrList<KeyValuePair<IMetadataContextKey, object?>, IReadOnlyCollection<KeyValuePair<IMetadataContextKey, object?>>>, IMetadataContext?>? TryGetMetadataContext { get; set; }

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        IReadOnlyMetadataContext? IMetadataContextProviderComponent.TryGetReadOnlyMetadataContext(IMetadataContextManager metadataContextManager, object? target, ItemOrList<KeyValuePair<IMetadataContextKey, object?>, IReadOnlyCollection<KeyValuePair<IMetadataContextKey, object?>>> values)
        {
            _metadataContextManager?.ShouldEqual(metadataContextManager);
            return TryGetReadOnlyMetadataContext?.Invoke(target, values);
        }

        IMetadataContext? IMetadataContextProviderComponent.TryGetMetadataContext(IMetadataContextManager metadataContextManager, object? target, ItemOrList<KeyValuePair<IMetadataContextKey, object?>, IReadOnlyCollection<KeyValuePair<IMetadataContextKey, object?>>> values)
        {
            _metadataContextManager?.ShouldEqual(metadataContextManager);
            return TryGetMetadataContext?.Invoke(target, values);
        }

        #endregion
    }
}