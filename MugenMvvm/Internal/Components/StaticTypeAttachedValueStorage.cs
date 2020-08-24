using System;
using System.Collections.Generic;
using MugenMvvm.Constants;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Internal.Components
{
    public sealed class StaticTypeAttachedValueStorage : AttachedValueStorageProviderBase, IHasPriority
    {
        #region Fields

        private readonly Dictionary<Type, SortedList<string, object?>> _attachedValues;

        #endregion

        #region Constructors

        public StaticTypeAttachedValueStorage()
        {
            _attachedValues = new Dictionary<Type, SortedList<string, object?>>(7, InternalEqualityComparer.Type);
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = InternalComponentPriority.StaticTypeAttachedValueProvider;

        #endregion

        #region Methods

        protected override bool IsSupported(IAttachedValueManager attachedValueManager, object item, IReadOnlyMetadataContext? metadata) => item is Type;

        protected override IDictionary<string, object?>? GetAttachedDictionary(object item, bool optional)
        {
            var type = (Type) item;
            lock (_attachedValues)
            {
                if (_attachedValues.TryGetValue(type, out var result) || optional)
                    return result;

                result = new SortedList<string, object?>(StringComparer.Ordinal);
                _attachedValues[type] = result;
                return result;
            }
        }

        protected override bool ClearInternal(object item)
        {
            lock (_attachedValues)
            {
                return _attachedValues.Remove((Type) item);
            }
        }

        #endregion
    }
}