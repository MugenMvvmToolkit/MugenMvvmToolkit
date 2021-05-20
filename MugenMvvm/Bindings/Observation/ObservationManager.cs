using System;
using MugenMvvm.Attributes;
using MugenMvvm.Bindings.Extensions.Components;
using MugenMvvm.Bindings.Interfaces.Observation;
using MugenMvvm.Bindings.Interfaces.Observation.Components;
using MugenMvvm.Collections;
using MugenMvvm.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Bindings.Observation
{
    public sealed class ObservationManager : ComponentOwnerBase<IObservationManager>, IObservationManager, IHasComponentAddedHandler, IHasComponentRemovedHandler
    {
        private readonly ComponentTracker _componentTracker;

        private ItemOrArray<IMemberObserverProviderComponent> _memberObserverComponents;
        private ItemOrArray<IMemberPathProviderComponent> _memberPathComponents;
        private ItemOrArray<IMemberPathObserverProviderComponent> _memberPathObserverComponents;

        [Preserve(Conditional = true)]
        public ObservationManager(IComponentCollectionManager? componentCollectionManager = null)
            : base(componentCollectionManager)
        {
            _componentTracker = new ComponentTracker();
            _componentTracker.AddListener<IMemberObserverProviderComponent, ObservationManager>((components, state, _) => state._memberObserverComponents = components, this);
            _componentTracker.AddListener<IMemberPathProviderComponent, ObservationManager>((components, state, _) => state._memberPathComponents = components, this);
            _componentTracker.AddListener<IMemberPathObserverProviderComponent, ObservationManager>((components, state, _) => state._memberPathObserverComponents = components,
                this);
        }

        public MemberObserver TryGetMemberObserver(Type type, object member, IReadOnlyMetadataContext? metadata = null) =>
            _memberObserverComponents.TryGetMemberObserver(this, type, member, metadata);

        public IMemberPath? TryGetMemberPath(object path, IReadOnlyMetadataContext? metadata = null) => _memberPathComponents.TryGetMemberPath(this, path, metadata);

        public IMemberPathObserver? TryGetMemberPathObserver(object target, object request, IReadOnlyMetadataContext? metadata = null) =>
            _memberPathObserverComponents.TryGetMemberPathObserver(this, target, request, metadata);

        void IHasComponentAddedHandler.OnComponentAdded(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata) =>
            _componentTracker.OnComponentChanged(collection, component, metadata);

        void IHasComponentRemovedHandler.OnComponentRemoved(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata) =>
            _componentTracker.OnComponentChanged(collection, component, metadata);
    }
}