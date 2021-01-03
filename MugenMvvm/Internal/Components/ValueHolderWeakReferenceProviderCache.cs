using MugenMvvm.Components;
using MugenMvvm.Constants;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Internal.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Internal.Components
{
    public sealed class ValueHolderWeakReferenceProviderCache : ComponentDecoratorBase<IWeakReferenceManager, IWeakReferenceProviderComponent>, IWeakReferenceProviderComponent
    {
        #region Constructors

        public ValueHolderWeakReferenceProviderCache(int priority = InternalComponentPriority.ValueHolderWeakReferenceCache) : base(priority)
        {
        }

        #endregion

        #region Implementation of interfaces

        public IWeakReference? TryGetWeakReference(IWeakReferenceManager weakReferenceManager, object item, IReadOnlyMetadataContext? metadata)
        {
            if (item is IValueHolder<IWeakReference> holder)
            {
                if (holder.Value == null)
                    holder.Value = Components.TryGetWeakReference(weakReferenceManager, item, metadata);
                return holder.Value;
            }

            return Components.TryGetWeakReference(weakReferenceManager, item, metadata);
        }

        #endregion
    }
}