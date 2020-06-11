using MugenMvvm.Collections;
using MugenMvvm.Collections.Internal;
using MugenMvvm.Constants;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Internal.Components
{
    public sealed class ValueHolderAttachedValueProvider : AttachedValueProviderBase, IHasPriority
    {
        #region Properties

        public int Priority { get; set; } = InternalComponentPriority.ValueHolderAttachedValueProvider;

        #endregion

        #region Methods

        public override bool IsSupported(object item, IReadOnlyMetadataContext? metadata)
        {
            return item is IValueHolder<LightDictionary<string, object?>>;
        }

        protected override LightDictionary<string, object?>? GetAttachedDictionary(object item, bool optional)
        {
            var holder = (IValueHolder<LightDictionary<string, object?>>) item;
            if (optional || holder.Value != null)
                return holder.Value;

            lock (holder)
            {
                if (holder.Value == null)
                    holder.Value = new StringOrdinalLightDictionary<object?>(3);
            }

            return holder.Value;
        }

        protected override bool ClearInternal(object item)
        {
            ((IValueHolder<LightDictionary<string, object?>>) item).Value = null;
            return true;
        }

        #endregion
    }
}