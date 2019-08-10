using System.Linq;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Components
{
    //todo add to global classes
    public sealed class ComponentTracker<TComponent, TComponentBase> : IComponentCollectionChangedListener<IComponent<TComponentBase>>
        where TComponent : class, IComponent
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

        int IComponent.GetPriority(object source)
        {
            return 0;
        }

        void IComponentCollectionChangedListener<IComponent<TComponentBase>>.OnAdded(IComponentCollection<IComponent<TComponentBase>> collection,
            IComponent<TComponentBase> component, IReadOnlyMetadataContext? metadata)
        {
            if (component is TComponent c)
                Add(c);
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

        private void Add(TComponent component)
        {
            var array = new TComponent[_items.Length + 1];
            var added = false;
            var priority = component.GetPriority(_owner!);
            for (var i = 0; i < _items.Length; i++)
            {
                if (added)
                {
                    array[i + 1] = _items[i];
                    continue;
                }

                var oldItem = _items[i];
                var compareTo = priority.CompareTo(oldItem.GetPriority(_owner!));
                if (compareTo > 0)
                {
                    array[i] = component;
                    added = true;
                    --i;
                }
                else
                    array[i] = oldItem;
            }

            if (!added)
                array[array.Length - 1] = component;
            _items = array;
        }

        private void Remove(TComponent component)
        {
            MugenExtensions.Remove(ref _items, component);
        }

        #endregion
    }
}