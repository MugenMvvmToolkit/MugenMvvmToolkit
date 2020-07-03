using System;
using System.Collections.Generic;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Metadata.Components;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;
using MugenMvvm.Metadata;

namespace MugenMvvm.UnitTest.Metadata.Internal
{
    public class TestMetadataContextProviderComponent : IMetadataContextProviderComponent, IHasPriority
    {
        #region Properties

        public Func<IMetadataContextManager, object?, ItemOrList<MetadataContextValue, IReadOnlyCollection<MetadataContextValue>>, IReadOnlyMetadataContext?>? TryGetReadOnlyMetadataContext { get; set; }

        public Func<IMetadataContextManager, object?, ItemOrList<MetadataContextValue, IReadOnlyCollection<MetadataContextValue>>, IMetadataContext?>? TryGetMetadataContext { get; set; }

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        IReadOnlyMetadataContext? IMetadataContextProviderComponent.TryGetReadOnlyMetadataContext(IMetadataContextManager metadataContextManager, object? target, ItemOrList<MetadataContextValue, IReadOnlyCollection<MetadataContextValue>> values)
        {
            return TryGetReadOnlyMetadataContext?.Invoke(metadataContextManager, target, values);
        }

        IMetadataContext? IMetadataContextProviderComponent.TryGetMetadataContext(IMetadataContextManager metadataContextManager, object? target, ItemOrList<MetadataContextValue, IReadOnlyCollection<MetadataContextValue>> values)
        {
            return TryGetMetadataContext?.Invoke(metadataContextManager, target, values);
        }

        #endregion
    }
}