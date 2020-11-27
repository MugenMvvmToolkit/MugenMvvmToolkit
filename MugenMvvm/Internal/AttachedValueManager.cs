using MugenMvvm.Components;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Internal.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Internal
{
    public sealed class AttachedValueManager : ComponentOwnerBase<IAttachedValueManager>, IAttachedValueManager, IHasComponentAddedHandler, IHasComponentRemovedHandler
    {
        #region Fields

        private readonly ComponentTracker _componentTracker;
        private IAttachedValueStorageProviderComponent[] _components;

        #endregion

        #region Constructors

        public AttachedValueManager(IComponentCollectionManager? componentCollectionManager = null) : base(componentCollectionManager)
        {
            _components = Default.Array<IAttachedValueStorageProviderComponent>();
            _componentTracker = new ComponentTracker();
            _componentTracker.AddListener<IAttachedValueStorageProviderComponent, AttachedValueManager>((components, state, _) => state._components = components, this);
        }

        #endregion

        #region Implementation of interfaces

        public AttachedValueStorage TryGetAttachedValues(object item, IReadOnlyMetadataContext? metadata = null) => _components.TryGetAttachedValues(this, item, metadata);

        void IHasComponentAddedHandler.OnComponentAdded(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata) => _componentTracker.OnComponentChanged(component, collection, metadata);

        void IHasComponentRemovedHandler.OnComponentRemoved(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata) => _componentTracker.OnComponentChanged(component, collection, metadata);

        #endregion
    }
}