using System.Collections.Generic;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Components
{
    public sealed class ComponentCollection<T> : IComponentCollection<T>, IComparer<T> where T : class
    {
        #region Fields

        private readonly List<T> _items;
        private T[]? _arrayItems;

        private IComponentCollection<IComponent<IComponentCollection<T>>>? _components;

        #endregion

        #region Constructors

        public ComponentCollection(object owner)
        {
            Owner = owner;
            _items = new List<T>();
        }

        #endregion

        #region Properties

        public object Owner { get; }

        public int Count => _items.Count;

        bool IComponentOwner<IComponentCollection<T>>.HasComponents => _components != null && _components.Count != 0;

        IComponentCollection<IComponent<IComponentCollection<T>>> IComponentOwner<IComponentCollection<T>>.Components
        {
            get
            {
                if (_components == null)
                    MugenService.ComponentCollectionProvider.LazyInitialize(ref _components, this);
                return _components!;
            }
        }

        #endregion

        #region Implementation of interfaces

        int IComparer<T>.Compare(T x, T y)
        {
            return MugenExtensions.GetComponentPriority(y, Owner).CompareTo(MugenExtensions.GetComponentPriority(x, Owner));
        }

        public T[] GetComponents()
        {
            return _arrayItems ?? GetItemsIfNeed();
        }

        public bool Add(T component, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(component, nameof(component));
            if (!MugenExtensions.OnAddingComponentHandler(this, component, metadata))
                return false;

            var components = MugenExtensions.GetComponents(this);
            for (var i = 0; i < components.Length; i++)
            {
                if (components[i] is IComponentCollectionChangingListener<T> listener && !listener.OnAdding(this, component, metadata))
                    return false;
            }

            lock (_items)
            {
                MugenExtensions.AddOrdered(_items, component, this);
                _arrayItems = null;
            }

            for (var i = 0; i < components.Length; i++)
                (components[i] as IComponentCollectionChangedListener<T>)?.OnAdded(this, component, metadata);
            MugenExtensions.OnAddedComponentHandler(this, component, metadata);
            return true;
        }

        public bool Remove(T component, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(component, nameof(component));
            lock (_items)
            {
                if (!_items.Contains(component))
                    return false;
            }

            if (!MugenExtensions.OnRemovingComponentHandler(this, component, metadata))
                return false;

            var components = MugenExtensions.GetComponents(this);
            for (var i = 0; i < components.Length; i++)
            {
                if (components[i] is IComponentCollectionChangingListener<T> listener && !listener.OnRemoving(this, component, metadata))
                    return false;
            }

            lock (_items)
            {
                if (!_items.Remove(component))
                    return false;
                _arrayItems = null;
            }

            for (var i = 0; i < components.Length; i++)
                (components[i] as IComponentCollectionChangedListener<T>)?.OnRemoved(this, component, metadata);
            MugenExtensions.OnRemovedComponentHandler(this, component, metadata);
            return true;
        }

        public bool Clear(IReadOnlyMetadataContext? metadata = null)
        {
            var components = MugenExtensions.GetComponents(this);
            var oldItems = GetComponents();
            lock (_items)
            {
                _items.Clear();
                _arrayItems = null;
            }

            for (var i = 0; i < oldItems.Length; i++)
            {
                var oldItem = oldItems[i];
                for (var j = 0; j < components.Length; j++)
                    (components[j] as IComponentCollectionChangedListener<T>)?.OnRemoved(this, oldItem, metadata);
                MugenExtensions.OnRemovedComponentHandler(this, oldItem, metadata);
            }

            return true;
        }

        #endregion

        #region Methods

        private T[] GetItemsIfNeed()
        {
            var items = _arrayItems;
            if (items == null)
            {
                lock (_items)
                {
                    items = _arrayItems;
                    if (items == null)
                    {
                        items = _items.ToArray();
                        _arrayItems = items;
                    }
                }
            }

            return items;
        }

        #endregion
    }
}