using MugenMvvm.Attributes;
using MugenMvvm.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Internal.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Internal
{
    public sealed class WeakReferenceProvider : ComponentOwnerBase<IWeakReferenceProvider>, IWeakReferenceProvider
    {
        #region Constructors

        [Preserve(Conditional = true)]
        public WeakReferenceProvider(IComponentCollectionProvider componentCollectionProvider) : base(componentCollectionProvider)
        {
        }

        #endregion

        #region Implementation of interfaces

        public IWeakReference GetWeakReference(object? item, IReadOnlyMetadataContext? metadata = null)
        {
            if (item == null)
                return Default.WeakReference;

            if (item is IWeakReference w)
                return w;

            var holder = item as IWeakReferenceHolder;
            if (holder != null)
            {
                var weakReference = holder.WeakReference;
                if (weakReference != null)
                    return weakReference;
            }

            var factories = Components.GetItems();
            for (var i = 0; i < factories.Length; i++)
            {
                var weakReference = (factories[i] as IWeakReferenceProviderComponent)?.TryGetWeakReference(item, metadata);
                if (weakReference != null)
                {
                    if (holder != null)
                        holder.WeakReference = weakReference;
                    return weakReference;
                }
            }

            ExceptionManager.ThrowObjectNotInitialized(this, typeof(IWeakReferenceProviderComponent).Name);
            return null!;
        }

        #endregion
    }
}