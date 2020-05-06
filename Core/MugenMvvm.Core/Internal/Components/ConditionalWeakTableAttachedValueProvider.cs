using System.Runtime.CompilerServices;
using MugenMvvm.Collections;
using MugenMvvm.Collections.Internal;
using MugenMvvm.Constants;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Internal.Components
{
    public sealed class ConditionalWeakTableAttachedValueProvider : AttachedValueProviderBase, IHasPriority
    {
        #region Fields

        private readonly ConditionalWeakTable<object, StringOrdinalLightDictionary<object?>> _weakTable;

        #endregion

        #region Constructors

        public ConditionalWeakTableAttachedValueProvider()
        {
            _weakTable = new ConditionalWeakTable<object, StringOrdinalLightDictionary<object?>>();
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = InternalComponentPriority.AttachedValueProvider;

        #endregion

        #region Methods

        public override bool IsSupported(object item, IReadOnlyMetadataContext? metadata)
        {
            return true;
        }

        protected override LightDictionary<string, object?>? GetAttachedDictionary(object item, bool optional)
        {
            if (optional)
            {
                _weakTable.TryGetValue(item, out var value);
                return value;
            }

            return _weakTable.GetValue(item, key => new StringOrdinalLightDictionary<object?>(3));
        }

        protected override bool ClearInternal(object item)
        {
            return _weakTable.Remove(item);
        }

        #endregion
    }
}