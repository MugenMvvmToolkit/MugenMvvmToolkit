using System;
using System.Collections.Generic;
using MugenMvvm.Constants;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Internal.Components
{
    public sealed class ValueHolderAttachedValueStorage : AttachedValueStorageProviderBase, IHasPriority
    {
        #region Properties

        public int Priority { get; set; } = InternalComponentPriority.ValueHolderAttachedValueProvider;

        #endregion

        #region Methods

        protected override bool IsSupported(IAttachedValueManager attachedValueManager, object item, IReadOnlyMetadataContext? metadata) => item is IValueHolder<IDictionary<string, object?>>;

        protected override IDictionary<string, object?>? GetAttachedDictionary(object item, bool optional)
        {
            var holder = (IValueHolder<IDictionary<string, object?>>) item;
            if (optional || holder.Value != null)
                return holder.Value;

            lock (holder)
            {
                if (holder.Value == null)
                    holder.Value = new SortedList<string, object?>(StringComparer.Ordinal);
            }

            return holder.Value;
        }

        protected override bool ClearInternal(object item)
        {
            ((IValueHolder<IDictionary<string, object?>>) item).Value = null;
            return true;
        }

        #endregion
    }
}