using MugenMvvm.Components;
using MugenMvvm.Constants;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Internal.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Internal.Components
{
    public sealed class ValueHolderWeakReferenceProviderCache : ComponentDecoratorBase<IWeakReferenceProvider, IWeakReferenceProviderComponent>, IWeakReferenceProviderComponent, IHasPriority
    {
        #region Properties

        public int Priority { get; set; } = ComponentPriority.Cache;

        #endregion

        #region Implementation of interfaces

        public IWeakReference? TryGetWeakReference(object item, IReadOnlyMetadataContext? metadata)
        {
            if (item is IValueHolder<IWeakReference> holder)
            {
                if (holder.Value == null)
                    holder.Value = Components.TryGetWeakReference(item, metadata);
                return holder.Value;
            }

            return Components.TryGetWeakReference(item, metadata);
        }

        #endregion
    }
}