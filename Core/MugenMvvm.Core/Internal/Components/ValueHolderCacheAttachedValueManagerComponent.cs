using MugenMvvm.Components;
using MugenMvvm.Constants;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Internal.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Internal.Components
{
    public sealed class ValueHolderCacheAttachedValueManagerComponent : DecoratorTrackerComponentBase<IAttachedValueManager, IAttachedValueManagerComponent>, IAttachedValueManagerComponent, IHasPriority
    {
        #region Properties

        public int Priority { get; set; } = ComponentPriority.Cache;

        #endregion

        #region Implementation of interfaces

        public bool TryGetAttachedValueProvider(object item, IReadOnlyMetadataContext? metadata, out IAttachedValueProvider? provider)
        {
            if (item is IValueHolder<IAttachedValueProvider> valueHolder)
            {
                provider = valueHolder.Value;
                return valueHolder.Value != null;
            }

            provider = null;
            return false;
        }

        public bool TryGetOrAddAttachedValueProvider(object item, IReadOnlyMetadataContext? metadata, out IAttachedValueProvider? provider)
        {
            if (item is IValueHolder<IAttachedValueProvider> valueHolder)
            {
                provider = GetAddAttachedValueProvider(item, metadata);
                valueHolder.Value = provider;
                return provider != null;
            }

            provider = null;
            return false;
        }

        #endregion

        #region Methods

        private IAttachedValueProvider? GetAddAttachedValueProvider(object item, IReadOnlyMetadataContext? metadata)
        {
            var components = Components;
            for (var i = 0; i < components.Length; i++)
            {
                if (components[i].TryGetAttachedValueProvider(item, metadata, out var provider))
                    return provider;
            }

            return null;
        }

        #endregion
    }
}