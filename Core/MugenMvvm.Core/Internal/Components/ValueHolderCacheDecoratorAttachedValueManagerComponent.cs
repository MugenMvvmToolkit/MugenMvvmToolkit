using MugenMvvm.Components;
using MugenMvvm.Constants;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Internal.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Internal.Components
{
    public sealed class ValueHolderCacheDecoratorAttachedValueManagerComponent : DecoratorComponentBase<IAttachedValueManager, IAttachedValueManagerComponent>, IAttachedValueManagerComponent, IHasPriority
    {
        #region Properties

        public int Priority { get; set; } = ComponentPriority.Cache;

        #endregion

        #region Implementation of interfaces

        public bool TryGetAttachedValueProvider(object item, IReadOnlyMetadataContext? metadata, out IAttachedValueProvider? provider)
        {
            if (!(item is IValueHolder<IAttachedValueProvider> valueHolder))
                return Components.TryGetAttachedValueProvider(item, metadata, out provider);

            if (valueHolder.Value == null)
            {
                if (!Components.TryGetAttachedValueProvider(item, metadata, out provider))
                    return false;
                valueHolder.Value = provider;
            }
            else
                provider = valueHolder.Value;

            return true;
        }

        public IAttachedValueProvider? TryGetOrAddAttachedValueProvider(object item, IReadOnlyMetadataContext? metadata)
        {
            if (!(item is IValueHolder<IAttachedValueProvider> valueHolder))
                return Components.TryGetOrAddAttachedValueProvider(item, metadata);

            var provider = valueHolder.Value;
            if (provider == null)
            {
                provider = Components.TryGetOrAddAttachedValueProvider(item, metadata);
                valueHolder.Value = provider;
            }

            return provider;
        }

        #endregion
    }
}