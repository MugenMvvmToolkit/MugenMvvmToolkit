using System;
using System.Collections.Generic;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Components
{
    public sealed class ComponentCollection : IComponentCollection, IComparer<object>, IComponentOwnerAddedCallback, IComponentOwnerRemovedCallback
    {
        #region Fields

        private readonly List<object> _items;

        private IDecoratorComponentCollectionComponent[] _decorators;
        private IComponentCollection? _components;
        private IComponentTracker[] _componentTrackers;

        #endregion

        #region Constructors

        public ComponentCollection(object owner)
        {
            Owner = owner;
            _items = new List<object>();
            _componentTrackers = Default.EmptyArray<IComponentTracker>();
            _decorators = Default.EmptyArray<IDecoratorComponentCollectionComponent>();
        }

        #endregion

        #region Properties

        public object Owner { get; }

        public int Count => _items.Count;

        bool IComponentOwner.HasComponents => _components != null && _components.Count != 0;

        IComponentCollection IComponentOwner.Components
        {
            get
            {
                if (_components == null)
                    MugenService.ComponentCollectionProvider.LazyInitialize(ref _components, this);
                return _components;
            }
        }

        #endregion

        #region Implementation of interfaces

        int IComparer<object>.Compare(object x, object y)
        {
            return MugenExtensions.GetComponentPriority(y, Owner).CompareTo(MugenExtensions.GetComponentPriority(x, Owner));
        }

        public bool Add(object component, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(component, nameof(component));
            if (!MugenExtensions.OnComponentAddingHandler(this, component, metadata))
                return false;

            var changingListeners = MugenExtensions.GetComponents<IComponentCollectionChangingListener>(this, metadata);
            for (var i = 0; i < changingListeners.Length; i++)
            {
                if (!changingListeners[i].OnAdding(this, component, metadata))
                    return false;
            }

            lock (_items)
            {
                MugenExtensions.AddOrdered(_items, component, this);
            }

            UpdateTrackers(component);
            var changedListeners = MugenExtensions.GetComponents<IComponentCollectionChangedListener>(this, metadata);
            for (var i = 0; i < changedListeners.Length; i++)
                changedListeners[i].OnAdded(this, component, metadata);
            MugenExtensions.OnComponentAddedHandler(this, component, metadata);
            return true;
        }

        public bool Remove(object component, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(component, nameof(component));
            lock (_items)
            {
                if (!_items.Contains(component))
                    return false;
            }

            if (!MugenExtensions.OnComponentRemovingHandler(this, component, metadata))
                return false;

            var changingListeners = MugenExtensions.GetComponents<IComponentCollectionChangingListener>(this, metadata);
            for (var i = 0; i < changingListeners.Length; i++)
            {
                if (!changingListeners[i].OnRemoving(this, component, metadata))
                    return false;
            }

            lock (_items)
            {
                if (!_items.Remove(component))
                    return false;
            }

            UpdateTrackers(component);
            var changedListeners = MugenExtensions.GetComponents<IComponentCollectionChangedListener>(this, metadata);
            for (var i = 0; i < changedListeners.Length; i++)
                changedListeners[i].OnRemoved(this, component, metadata);
            MugenExtensions.OnComponentRemovedHandler(this, component, metadata);
            return true;
        }

        public bool Clear(IReadOnlyMetadataContext? metadata = null)
        {
            var oldItems = GetComponents<object>(metadata);
            lock (_items)
            {
                _items.Clear();
            }

            _componentTrackers = Default.EmptyArray<IComponentTracker>();
            var changedListeners = MugenExtensions.GetComponents<IComponentCollectionChangedListener>(this, metadata);
            for (var i = 0; i < oldItems.Length; i++)
            {
                var oldItem = oldItems[i];
                for (var j = 0; j < changedListeners.Length; j++)
                    changedListeners[j].OnRemoved(this, oldItem, metadata);
                MugenExtensions.OnComponentRemovedHandler(this, oldItem, metadata);
            }

            return true;
        }

        public TComponent[] GetComponents<TComponent>(IReadOnlyMetadataContext? metadata) where TComponent : class
        {
            var componentTrackers = _componentTrackers;
            for (var i = 0; i < componentTrackers.Length; i++)
            {
                if (componentTrackers[i] is ComponentTracker<TComponent> tracker)
                {
                    if (_decorators.Length == 0 || typeof(object) == typeof(TComponent))
                        return tracker.Components;
                    return Decorate(tracker.Components, metadata);
                }
            }

            return AddNewTracker<TComponent>(componentTrackers, metadata);
        }

        void IComponentOwnerAddedCallback.OnComponentAdded(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata)
        {
            if (component is IDecoratorComponentCollectionComponent decorator)
                MugenExtensions.AddComponentOrdered(ref _decorators, decorator, this);
        }

        void IComponentOwnerRemovedCallback.OnComponentRemoved(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata)
        {
            if (component is IDecoratorComponentCollectionComponent decorator)
                MugenExtensions.Remove(ref _decorators, decorator);
        }

        #endregion

        #region Methods

        private TComponent[] AddNewTracker<TComponent>(IComponentTracker[] componentTrackers, IReadOnlyMetadataContext? metadata) where TComponent : class
        {
            var tracker = ComponentTracker<TComponent>.Get(this);
            Array.Resize(ref componentTrackers, componentTrackers.Length + 1);
            componentTrackers[componentTrackers.Length - 1] = tracker;
            _componentTrackers = componentTrackers;
            if (_decorators.Length == 0 || typeof(object) == typeof(TComponent))
                return tracker.Components;
            return Decorate(tracker.Components, metadata);
        }

        private TComponent[] Decorate<TComponent>(TComponent[] components, IReadOnlyMetadataContext? metadata) where TComponent : class
        {
            var decorators = _decorators;
            for (var i = 0; i < decorators.Length; i++)
            {
                if (decorators[i] is IDecoratorComponentCollectionComponent<TComponent> decorator && decorator.TryDecorate(ref components, metadata))
                    break;
            }

            return components;
        }

        private void UpdateTrackers(object component)
        {
            var componentTrackers = _componentTrackers;
            int newSize = 0;
            for (int i = 0; i < componentTrackers.Length; i++)
            {
                var componentTracker = componentTrackers[i];
                if (!componentTracker.IsComponent(component))
                    componentTrackers[newSize++] = componentTracker;
            }

            if (newSize == 0)
                _componentTrackers = Default.EmptyArray<IComponentTracker>();
            else if (newSize != componentTrackers.Length)
                Array.Resize(ref _componentTrackers, newSize);
        }

        #endregion

        #region Nested types

        public interface IComponentTracker
        {
            bool IsComponent(object component);
        }

        private sealed class ComponentTracker<TComponent> : IComponentTracker
            where TComponent : class
        {
            #region Fields

            public readonly TComponent[] Components;

            private static readonly ComponentTracker<TComponent> Empty = new ComponentTracker<TComponent>(Default.EmptyArray<TComponent>());

            #endregion

            #region Constructors

            private ComponentTracker(TComponent[] components)
            {
                Components = components;
            }

            #endregion

            #region Implementation of interfaces

            public bool IsComponent(object component)
            {
                return component is TComponent;
            }

            #endregion

            #region Methods

            public static ComponentTracker<TComponent> Get(ComponentCollection collection)
            {
                var items = collection._items;
                lock (items)
                {
                    var size = 0;
                    for (var i = 0; i < items.Count; i++)
                    {
                        if (items[i] is TComponent)
                            ++size;
                    }

                    if (size == 0)
                        return Empty;

                    var components = new TComponent[size];
                    size = 0;
                    for (var i = 0; i < items.Count; i++)
                    {
                        if (items[i] is TComponent c)
                            components[size++] = c;
                    }

                    return new ComponentTracker<TComponent>(components);
                }
            }

            #endregion
        }

        #endregion
    }
}