using MugenMvvm.Attributes;
using MugenMvvm.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Internal.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Internal
{
    public sealed class WeakReferenceProvider : ComponentOwnerBase<IWeakReferenceProvider>, IWeakReferenceProvider,
        IComponentOwnerAddedCallback<IComponent<IWeakReferenceProvider>>, IComponentOwnerRemovedCallback<IComponent<IWeakReferenceProvider>>
    {
        #region Fields

        private IWeakReferenceProviderComponent[] _providers;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public WeakReferenceProvider(IComponentCollectionProvider? componentCollectionProvider = null) : base(componentCollectionProvider)
        {
            _providers = Default.EmptyArray<IWeakReferenceProviderComponent>();
        }

        #endregion

        #region Implementation of interfaces

        void IComponentOwnerAddedCallback<IComponent<IWeakReferenceProvider>>.OnComponentAdded(IComponentCollection<IComponent<IWeakReferenceProvider>> collection,
            IComponent<IWeakReferenceProvider> component, IReadOnlyMetadataContext? metadata)
        {
            MugenExtensions.ComponentTrackerOnAdded(ref _providers, collection, component);
        }

        void IComponentOwnerRemovedCallback<IComponent<IWeakReferenceProvider>>.OnComponentRemoved(IComponentCollection<IComponent<IWeakReferenceProvider>> collection,
            IComponent<IWeakReferenceProvider> component, IReadOnlyMetadataContext? metadata)
        {
            MugenExtensions.ComponentTrackerOnRemoved(ref _providers, component);
        }

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

            var providers = _providers;
            for (var i = 0; i < providers.Length; i++)
            {
                var weakReference = providers[i].TryGetWeakReference(item, metadata);
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