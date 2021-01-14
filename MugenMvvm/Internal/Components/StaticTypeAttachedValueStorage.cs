using System;
using System.Collections.Generic;
using MugenMvvm.Constants;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Internal.Components
{
    public sealed class StaticTypeAttachedValueStorage : AttachedValueStorageProviderBase<Type>, IHasPriority
    {
        private readonly Dictionary<Type, SortedList<string, object?>> _attachedValues;

        public StaticTypeAttachedValueStorage()
        {
            _attachedValues = new Dictionary<Type, SortedList<string, object?>>(7, InternalEqualityComparer.Type);
        }

        public int Priority { get; set; } = InternalComponentPriority.StaticTypeAttachedValueProvider;

        protected override IDictionary<string, object?>? GetAttachedDictionary(Type item, bool optional)
        {
            lock (_attachedValues)
            {
                if (_attachedValues.TryGetValue(item, out var result) || optional)
                    return result;

                result = new SortedList<string, object?>(3, StringComparer.Ordinal);
                _attachedValues[item] = result;
                return result;
            }
        }

        protected override bool ClearInternal(Type item)
        {
            lock (_attachedValues)
            {
                return _attachedValues.Remove(item);
            }
        }
    }
}