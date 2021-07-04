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
    public sealed class ConditionalWeakTableAttachedValueStorage : AttachedValueStorageProviderBase<object>, IHasPriority
    {
        private readonly ConditionalWeakTable<object, SortedList<string, object?>> _weakTable;

        [Preserve(Conditional = true)]
        public ConditionalWeakTableAttachedValueStorage()
        {
            _weakTable = new ConditionalWeakTable<object, SortedList<string, object?>>();
        }

        public int Priority { get; init; } = InternalComponentPriority.WeakTableAttachedValueProvider;

        protected override IDictionary<string, object?>? GetAttachedDictionary(object item, bool optional)
        {
            if (optional)
            {
                _weakTable.TryGetValue(item, out var value);
                return value;
            }

            return _weakTable.GetValue(item, key => new SortedList<string, object?>(3, StringComparer.Ordinal));
        }

        protected override bool ClearInternal(object item) => _weakTable.Remove(item);

        protected override bool IsSupported(IAttachedValueManager attachedValueManager, object item, IReadOnlyMetadataContext? metadata) => true;
    }
}