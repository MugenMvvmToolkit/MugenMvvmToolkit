using System;
using MugenMvvm.Bindings.Extensions.Components;
using MugenMvvm.Bindings.Interfaces.Observation;
using MugenMvvm.Bindings.Interfaces.Observation.Components;
using MugenMvvm.Collections;
using MugenMvvm.Components;
using MugenMvvm.Constants;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Internal.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models.Components;

namespace MugenMvvm.Bindings.Observation.Components
{
    public class SynchronizedObservationManagerDecorator : ComponentDecoratorBase<IObservationManager, IMemberPathObserverProviderComponent>, IMemberPathObserverProviderComponent,
        IMemberPathProviderComponent, IMemberObserverProviderComponent, IHasCacheComponent<IObservationManager>,
        IComponentCollectionDecorator<IHasCacheComponent<IObservationManager>>, IComponentCollectionDecorator<IMemberPathProviderComponent>,
        IComponentCollectionDecorator<IMemberObserverProviderComponent>, ISynchronizedComponent<IObservationManager>
    {
        private ItemOrArray<IHasCacheComponent<IObservationManager>> _cacheComponents;
        private ItemOrArray<IMemberPathProviderComponent> _pathProviderComponents;
        private ItemOrArray<IMemberObserverProviderComponent> _observerProviderComponents;
        private readonly object _syncRoot;

        public SynchronizedObservationManagerDecorator(object? syncRoot = null, int priority = ComponentPriority.Synchronizer) : base(priority)
        {
            _syncRoot = syncRoot ?? this;
        }

        public object SyncRoot => _syncRoot;

        void IComponentCollectionDecorator<IHasCacheComponent<IObservationManager>>.Decorate(IComponentCollection collection,
            ref ItemOrListEditor<IHasCacheComponent<IObservationManager>> components, IReadOnlyMetadataContext? metadata) =>
            _cacheComponents = this.Decorate(ref components);

        void IComponentCollectionDecorator<IMemberObserverProviderComponent>.Decorate(IComponentCollection collection,
            ref ItemOrListEditor<IMemberObserverProviderComponent> components, IReadOnlyMetadataContext? metadata) =>
            _observerProviderComponents = this.Decorate(ref components);

        void IComponentCollectionDecorator<IMemberPathProviderComponent>.Decorate(IComponentCollection collection, ref ItemOrListEditor<IMemberPathProviderComponent> components,
            IReadOnlyMetadataContext? metadata) =>
            _pathProviderComponents = this.Decorate(ref components);

        void IHasCacheComponent<IObservationManager>.Invalidate(IObservationManager owner, object? state, IReadOnlyMetadataContext? metadata)
        {
            lock (_syncRoot)
            {
                _cacheComponents.Invalidate(owner, state, metadata);
            }
        }

        MemberObserver IMemberObserverProviderComponent.TryGetMemberObserver(IObservationManager observationManager, Type type, object member, IReadOnlyMetadataContext? metadata)
        {
            lock (_syncRoot)
            {
                return _observerProviderComponents.TryGetMemberObserver(observationManager, type, member, metadata);
            }
        }

        IMemberPathObserver? IMemberPathObserverProviderComponent.TryGetMemberPathObserver(IObservationManager observationManager, object target, object request,
            IReadOnlyMetadataContext? metadata)
        {
            lock (_syncRoot)
            {
                return Components.TryGetMemberPathObserver(observationManager, target, request, metadata);
            }
        }

        IMemberPath? IMemberPathProviderComponent.TryGetMemberPath(IObservationManager observationManager, object path, IReadOnlyMetadataContext? metadata)
        {
            lock (_syncRoot)
            {
                return _pathProviderComponents.TryGetMemberPath(observationManager, path, metadata);
            }
        }
    }
}