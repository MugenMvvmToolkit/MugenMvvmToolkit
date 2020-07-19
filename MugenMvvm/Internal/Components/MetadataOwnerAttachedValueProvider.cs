﻿using System;
using System.Collections.Generic;
using MugenMvvm.Constants;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Metadata;

namespace MugenMvvm.Internal.Components
{
    public sealed class MetadataOwnerAttachedValueProvider : AttachedValueProviderBase, IHasPriority
    {
        #region Fields

        private static readonly IMetadataContextKey<SortedList<string, object?>, SortedList<string, object?>> Key = MetadataContextKey.FromMember(Key, typeof(MetadataOwnerAttachedValueProvider), nameof(Key));

        #endregion

        #region Properties

        public int Priority { get; set; } = InternalComponentPriority.MetadataOwnerAttachedValueProvider;

        #endregion

        #region Methods

        public override bool IsSupported(IAttachedValueManager attachedValueManager, object item, IReadOnlyMetadataContext? metadata)
        {
            return item is IMetadataOwner<IMetadataContext>;
        }

        protected override IDictionary<string, object?>? GetAttachedDictionary(object item, bool optional)
        {
            if (!optional)
                return ((IMetadataOwner<IMetadataContext>)item).Metadata.GetOrAdd(Key, this, (context, provider) => new SortedList<string, object?>(StringComparer.Ordinal));
            var owner = (IMetadataOwner<IReadOnlyMetadataContext>)item;
            if (owner.HasMetadata)
                return owner.Metadata.Get(Key);
            return null;
        }

        protected override bool ClearInternal(object item)
        {
            var owner = (IMetadataOwner<IMetadataContext>)item;
            if (owner.HasMetadata)
                return owner.Metadata.Remove(Key, out _);
            return false;
        }

        #endregion
    }
}