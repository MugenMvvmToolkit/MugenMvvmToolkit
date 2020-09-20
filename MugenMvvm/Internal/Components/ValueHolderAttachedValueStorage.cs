using System;
using System.Collections.Generic;
using MugenMvvm.Constants;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Internal.Components
{
    public sealed class ValueHolderAttachedValueStorage : AttachedValueStorageProviderBase<IValueHolder<IDictionary<string, object?>>>, IHasPriority
    {
        #region Properties

        public int Priority { get; set; } = InternalComponentPriority.ValueHolderAttachedValueProvider;

        #endregion

        #region Methods

        protected override IDictionary<string, object?>? GetAttachedDictionary(IValueHolder<IDictionary<string, object?>> item, bool optional)
        {
            if (optional || item.Value != null)
                return item.Value;

            lock (item)
            {
                item.Value ??= new SortedList<string, object?>(3, StringComparer.Ordinal);
            }

            return item.Value;
        }

        protected override bool ClearInternal(IValueHolder<IDictionary<string, object?>> item)
        {
            item.Value = null;
            return true;
        }

        #endregion
    }
}