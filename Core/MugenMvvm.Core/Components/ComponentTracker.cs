using System.Linq;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Components
{
    //todo add to global classes
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
            if (component is TComponent c)
                MugenExtensions.AddOrdered(ref _items, c, _owner!);
        }

        void IComponentCollectionChangedListener<IComponent<TComponentBase>>.OnRemoved(IComponentCollection<IComponent<TComponentBase>> collection,
            IComponent<TComponentBase> component, IReadOnlyMetadataContext? metadata)
        {
            if (component is TComponent c)
                Remove(c);
        }

        void IComponentCollectionChangedListener<IComponent<TComponentBase>>.OnCleared(IComponentCollection<IComponent<TComponentBase>> collection,
            IComponent<TComponentBase>[] oldItems, IReadOnlyMetadataContext? metadata)
        {
            for (var index = 0; index < oldItems.Length; index++)
            {
                if (oldItems[index] is TComponent c)
                    Remove(c);
            }
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

        private void Remove(TComponent component)
        {
            MugenExtensions.Remove(ref _items, component);
        }

        #endregion
    }
}