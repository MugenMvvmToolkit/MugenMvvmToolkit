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
        public WeakReferenceProvider(IComponentCollectionProvider? componentCollectionProvider = null) : base(componentCollectionProvider)
        {
        }

        #endregion

        #region Implementation of interfaces

        public IWeakReference GetWeakReference(object? item, IReadOnlyMetadataContext? metadata = null)
        {
            if (item == null)
                return Default.WeakReference;

            var providers = GetComponents<IWeakReferenceProviderComponent>(metadata);
            for (var i = 0; i < providers.Length; i++)
            {
                var weakReference = providers[i].TryGetWeakReference(item, metadata);
                if (weakReference != null)
                    return weakReference;
            }

            ExceptionManager.ThrowObjectNotInitialized(this, providers);
            return null;
        }

        #endregion
    }
}