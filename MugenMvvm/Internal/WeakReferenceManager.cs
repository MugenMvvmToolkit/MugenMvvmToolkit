using MugenMvvm.Attributes;
using MugenMvvm.Collections;
using MugenMvvm.Components;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Internal.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Internal
{
    public sealed class WeakReferenceManager : ComponentOwnerBase<IWeakReferenceManager>, IWeakReferenceManager, IHasComponentAddedHandler, IHasComponentRemovedHandler
    {
        private readonly ComponentTracker _componentTracker;
        private ItemOrArray<IWeakReferenceProviderComponent> _components;

        [Preserve(Conditional = true)]
        public WeakReferenceManager(IComponentCollectionManager? componentCollectionManager = null) : base(componentCollectionManager)
        {
            _componentTracker = new ComponentTracker();
            _componentTracker.AddListener<IWeakReferenceProviderComponent, WeakReferenceManager>((components, state, _) => state._components = components, this);
        }

        public IWeakReference? TryGetWeakReference(object? item, IReadOnlyMetadataContext? metadata = null)
        {
            if (item == null)
                return WeakReferenceImpl.Empty;
            return _components.TryGetWeakReference(this, item, metadata);
        }

        void IHasComponentAddedHandler.OnComponentAdded(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata) =>
            _componentTracker.OnComponentChanged(component, collection, metadata);

        void IHasComponentRemovedHandler.OnComponentRemoved(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata) =>
            _componentTracker.OnComponentChanged(component, collection, metadata);
    }
}