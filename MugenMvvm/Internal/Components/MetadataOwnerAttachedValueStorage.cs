using System;
using System.Collections.Generic;
using MugenMvvm.Constants;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Metadata;

namespace MugenMvvm.Internal.Components
{
    public sealed class MetadataOwnerAttachedValueStorage : AttachedValueStorageProviderBase<IMetadataOwner<IMetadataContext>>, IHasPriority
    {
        public int Priority { get; init; } = InternalComponentPriority.MetadataOwnerAttachedValueProvider;

        protected override IDictionary<string, object?>? GetAttachedDictionary(IMetadataOwner<IMetadataContext> item, bool optional)
        {
            if (!optional)
                return item.Metadata.GetOrAdd(InternalMetadata.AttachedValuesKey, this, (context, key, provider) => new SortedList<string, object?>(3, StringComparer.Ordinal));
            if (item.HasMetadata)
                return item.Metadata.Get(InternalMetadata.AttachedValuesKey);
            return null;
        }

        protected override bool ClearInternal(IMetadataOwner<IMetadataContext> item)
        {
            if (item.HasMetadata)
                return item.Metadata.Remove(InternalMetadata.AttachedValuesKey, out _);
            return false;
        }
    }
}