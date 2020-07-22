using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MugenMvvm.Attributes;
using MugenMvvm.Constants;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Internal.Components
{
    public sealed class ConditionalWeakTableAttachedValueStorage : AttachedValueStorageProviderBase, IHasPriority
    {
        #region Fields

        private readonly ConditionalWeakTable<object, SortedList<string, object?>> _weakTable;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public ConditionalWeakTableAttachedValueStorage()
        {
            _weakTable = new ConditionalWeakTable<object, SortedList<string, object?>>();
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = InternalComponentPriority.WeakTable;

        #endregion

        #region Methods

        protected override bool IsSupported(IAttachedValueManager attachedValueManager, object item, IReadOnlyMetadataContext? metadata)
        {
            return true;
        }

        protected override IDictionary<string, object?>? GetAttachedDictionary(object item, bool optional)
        {
            if (optional)
            {
                _weakTable.TryGetValue(item, out var value);
                return value;
            }

            return _weakTable.GetValue(item, key => new SortedList<string, object?>(StringComparer.Ordinal));
        }

        protected override bool ClearInternal(object item)
        {
            return _weakTable.Remove(item);
        }

        #endregion
    }
}