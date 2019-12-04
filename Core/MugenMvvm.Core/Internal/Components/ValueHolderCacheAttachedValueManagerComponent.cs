using MugenMvvm.Components;
using MugenMvvm.Constants;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Internal.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Internal.Components
{
    public sealed class ValueHolderCacheAttachedValueManagerComponent : DecoratorComponentBase<IAttachedValueManager, IAttachedValueManagerComponent>, IAttachedValueManagerComponent, IHasPriority
    {
        #region Properties

        public int Priority { get; set; } = ComponentPriority.Cache;

        #endregion

        #region Implementation of interfaces

        public bool TryGetAttachedValueProvider(object item, IReadOnlyMetadataContext? metadata, out IAttachedValueProvider? provider)
        {
            if (!(item is IValueHolder<IAttachedValueProvider> valueHolder))
                return TryGetAttachedValueProviderInternal(item, metadata, out provider);

            if (valueHolder.Value == null)
            {
                if (!TryGetAttachedValueProviderInternal(item, metadata, out provider))
                    return false;
                valueHolder.Value = provider;
            }
            else
                provider = valueHolder.Value;

            return true;
        }

        public bool TryGetOrAddAttachedValueProvider(object item, IReadOnlyMetadataContext? metadata, out IAttachedValueProvider? provider)
        {
            if (!(item is IValueHolder<IAttachedValueProvider> valueHolder))
                return TryGetOrAddAttachedValueProviderInternal(item, metadata, out provider);

            if (valueHolder.Value == null)
            {
                if (!TryGetOrAddAttachedValueProviderInternal(item, metadata, out provider))
                    return false;
                valueHolder.Value = provider;
            }
            else
                provider = valueHolder.Value;

            return true;
        }

        #endregion

        #region Methods

        private bool TryGetOrAddAttachedValueProviderInternal(object item, IReadOnlyMetadataContext? metadata, out IAttachedValueProvider? provider)
        {
            var components = Components;
            for (var i = 0; i < components.Length; i++)
            {
                if (components[i].TryGetOrAddAttachedValueProvider(item, metadata, out provider))
                    return true;
            }

            provider = null;
            return false;
        }

        private bool TryGetAttachedValueProviderInternal(object item, IReadOnlyMetadataContext? metadata, out IAttachedValueProvider? provider)
        {
            var components = Components;
            for (var i = 0; i < components.Length; i++)
            {
                if (components[i].TryGetAttachedValueProvider(item, metadata, out provider))
                    return true;
            }

            provider = null;
            return false;
        }

        #endregion
    }
}