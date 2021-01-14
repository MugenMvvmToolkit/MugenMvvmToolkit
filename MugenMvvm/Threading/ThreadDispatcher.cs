using MugenMvvm.Collections;
using MugenMvvm.Components;
using MugenMvvm.Enums;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Threading;
using MugenMvvm.Interfaces.Threading.Components;

namespace MugenMvvm.Threading
{
    public sealed class ThreadDispatcher : ComponentOwnerBase<IThreadDispatcher>, IThreadDispatcher, IHasComponentAddedHandler, IHasComponentRemovedHandler
    {
        private readonly ComponentTracker _componentTracker;
        private ItemOrArray<IThreadDispatcherComponent> _components;

        public ThreadDispatcher(IComponentCollectionManager? componentCollectionManager = null) : base(componentCollectionManager)
        {
            _componentTracker = new ComponentTracker();
            _componentTracker.AddListener<IThreadDispatcherComponent, ThreadDispatcher>((components, state, _) => state._components = components, this);
        }

        public bool CanExecuteInline(ThreadExecutionMode executionMode, IReadOnlyMetadataContext? metadata = null) => _components.CanExecuteInline(this, executionMode, metadata);

        public bool TryExecute(ThreadExecutionMode executionMode, object handler, object? state = null, IReadOnlyMetadataContext? metadata = null) =>
            _components.TryExecute(this, executionMode, handler, state, metadata);

        void IHasComponentAddedHandler.OnComponentAdded(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata) =>
            _componentTracker.OnComponentChanged(component, collection, metadata);

        void IHasComponentRemovedHandler.OnComponentRemoved(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata) =>
            _componentTracker.OnComponentChanged(component, collection, metadata);
    }
}