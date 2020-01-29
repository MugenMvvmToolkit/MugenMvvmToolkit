using System;
using System.Collections.Generic;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Metadata.Components;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;
using MugenMvvm.Metadata;

namespace MugenMvvm.UnitTest.Metadata
{
    public class TestMetadataContextProviderComponent : IMetadataContextProviderComponent, IHasPriority
    {
        #region Properties

        public Func<object?, ItemOrList<MetadataContextValue, IReadOnlyCollection<MetadataContextValue>>, IReadOnlyMetadataContext?>? TryGetReadOnlyMetadataContext { get; set; }

        public Func<object?, ItemOrList<MetadataContextValue, IReadOnlyCollection<MetadataContextValue>>, IMetadataContext?>? TryGetMetadataContext { get; set; }

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        IReadOnlyMetadataContext? IMetadataContextProviderComponent.TryGetReadOnlyMetadataContext(object? target, ItemOrList<MetadataContextValue, IReadOnlyCollection<MetadataContextValue>> values)
        {
            return TryGetReadOnlyMetadataContext?.Invoke(target, values);
        }

        IMetadataContext? IMetadataContextProviderComponent.TryGetMetadataContext(object? target, ItemOrList<MetadataContextValue, IReadOnlyCollection<MetadataContextValue>> values)
        {
            return TryGetMetadataContext?.Invoke(target, values);
        }

        #endregion
    }
}