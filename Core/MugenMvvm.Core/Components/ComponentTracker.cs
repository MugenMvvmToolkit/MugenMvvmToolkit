using System.Linq;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Components
{
    public sealed class ComponentTracker<TComponent, TComponentBase> : IComponentCollectionChangedListener<IComponent<TComponentBase>>
        where TComponent : class
        where TComponentBase : class
    {
        #region Fields

        private TComponent[] _items;
        private IComponentOwner<TComponentBase>? _owner;

        #endregion

        #region Constructors

        public ComponentTracker()
        {
            _items = Default.EmptyArray<TComponent>();
        }

        #endregion

        #region Implementation of interfaces

        void IComponentCollectionChangedListener<IComponent<TComponentBase>>.OnAdded(IComponentCollection<IComponent<TComponentBase>> collection,
            IComponent<TComponentBase> component, IReadOnlyMetadataContext? metadata)
        {
            MugenExtensions.ComponentTrackerOnAdded(ref _items, _owner!, collection, component, metadata);
        }

        void IComponentCollectionChangedListener<IComponent<TComponentBase>>.OnRemoved(IComponentCollection<IComponent<TComponentBase>> collection,
            IComponent<TComponentBase> component, IReadOnlyMetadataContext? metadata)
        {
            MugenExtensions.ComponentTrackerOnRemoved(ref _items, collection, component, metadata);
        }

        void IComponentCollectionChangedListener<IComponent<TComponentBase>>.OnCleared(IComponentCollection<IComponent<TComponentBase>> collection,
            IComponent<TComponentBase>[] oldItems, IReadOnlyMetadataContext? metadata)
        {
            MugenExtensions.ComponentTrackerOnCleared(ref _items, collection, oldItems, metadata);
        }

        #endregion

        #region Methods

        public void Attach(IComponentOwner<TComponentBase> owner)
        {
            Should.NotBeNull(owner, nameof(owner));
            _owner?.Components.Components.Remove(this);
            _owner = owner;
            _items = owner.Components.GetItems().OfType<TComponent>().ToArray();
            _owner.Components.Components.Add(this);
        }

        public TComponent[] GetComponents()
        {
            return _items;
        }

        #endregion
    }
}