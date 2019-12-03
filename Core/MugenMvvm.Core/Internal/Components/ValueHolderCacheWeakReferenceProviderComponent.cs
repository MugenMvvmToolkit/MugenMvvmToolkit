using MugenMvvm.Components;
using MugenMvvm.Constants;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Internal.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Internal.Components
{
    public sealed class ValueHolderCacheWeakReferenceProviderComponent : DecoratorTrackerComponentBase<IWeakReferenceProvider, IWeakReferenceProviderComponent>, IWeakReferenceProviderComponent, IHasPriority
    {
        #region Properties

        public int Priority { get; set; } = ComponentPriority.Cache;

        #endregion

        #region Implementation of interfaces

        public IWeakReference? TryGetWeakReference(object item, IReadOnlyMetadataContext? metadata)
        {
            if (item is IWeakReference w)
                return w;

            if (item is IValueHolder<IWeakReference> holder)
            {
                if (holder.Value == null)
                    holder.Value = GetWeakReference(item, metadata);
                return holder.Value;
            }

            return null;
        }

        #endregion

        #region Methods

        private IWeakReference? GetWeakReference(object item, IReadOnlyMetadataContext? metadata)
        {
            var providers = Components;
            for (var i = 0; i < providers.Length; i++)
            {
                var weakReference = providers[i].TryGetWeakReference(item, metadata);
                if (weakReference != null)
                    return weakReference;
            }

            return null;
        }

        #endregion
    }
}