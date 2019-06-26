using MugenMvvm.Attributes;
using MugenMvvm.Infrastructure.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Infrastructure.Internal
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

        public IWeakReference GetWeakReference(object item, IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(metadata, nameof(metadata));

            if (item == null)
                return Default.WeakReference;

            if (item is IWeakReference w)
                return w;

            var factories = Components.GetItems();
            for (var i = 0; i < factories.Length; i++)
            {
                var weakReference = (factories[i] as IWeakReferenceProviderComponent)?.TryGetWeakReference(item, metadata);
                if (weakReference != null)
                    return weakReference;
            }

            ExceptionManager.ThrowObjectNotInitialized(this, typeof(IWeakReferenceProviderComponent).Name);
            return null;
        }

        #endregion
    }
}