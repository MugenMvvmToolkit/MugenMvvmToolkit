using System.Collections.Generic;
using MugenMvvm.Constants;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Metadata.Components;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;

namespace MugenMvvm.Metadata.Components
{
    public sealed class MetadataContextProviderComponent : IMetadataContextProviderComponent, IHasPriority
    {
        #region Properties

        public int Priority { get; set; } = MetadataComponentPriority.MetadataProvider;

        #endregion

        #region Implementation of interfaces

        public IReadOnlyMetadataContext? TryGetReadOnlyMetadataContext(object? target, ItemOrList<MetadataContextValue, IReadOnlyCollection<MetadataContextValue>> values)
        {
            var list = values.List;
            var item = values.Item;
            if (list == null && item.IsEmpty)
                return Default.Metadata;
            if (list == null)
                return new SingleValueMetadataContext(item);
            return new ReadOnlyMetadataContext(list);
        }

        public IMetadataContext? TryGetMetadataContext(object? target, ItemOrList<MetadataContextValue, IReadOnlyCollection<MetadataContextValue>> values)
        {
            return new MetadataContext(values);
        }

        #endregion
    }
}