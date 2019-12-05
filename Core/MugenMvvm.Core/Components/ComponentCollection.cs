using System;
using System.Collections.Generic;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Components
{
    public sealed class ComponentCollection : IComponentCollection, IComparer<object>, IHasAddedCallbackComponentOwner, IHasRemovedCallbackComponentOwner, IComparer<IDecoratorComponentCollectionComponent>
    {
        #region Fields

        private readonly List<object> _items;
        private IComponentCollection? _components;
        private IComponentTracker[] _componentTrackers;

        private IDecoratorComponentCollectionComponent[] _decorators;

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

        int IComparer<IDecoratorComponentCollectionComponent>.Compare(IDecoratorComponentCollectionComponent x, IDecoratorComponentCollectionComponent y)
        {
            return MugenExtensions.GetComponentPriority(x, this).CompareTo(MugenExtensions.GetComponentPriority(y, this));
        }

        int IComparer<object>.Compare(object x, object y)
        {
            return MugenExtensions.GetComponentPriority(y, Owner).CompareTo(MugenExtensions.GetComponentPriority(x, Owner));
        }

        public bool Add(object component, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(component, nameof(component));
            if (!MugenExtensions.ComponentCollectionOnComponentAdding(this, component, metadata))
                return false;

            if (_components != null)
            {
                var changingListeners = _components.GetComponents<IComponentCollectionChangingListener>(metadata);
                for (var i = 0; i < changingListeners.Length; i++)
                {
                    if (!changingListeners[i].OnAdding(this, component, metadata))
                        return false;
                }
            }

            lock (_items)
            {
                MugenExtensions.AddOrdered(_items, component, this);
            }

            UpdateTrackers(component);

            if (_components != null)
            {
                var changedListeners = _components.GetComponents<IComponentCollectionChangedListener>(metadata);
                for (var i = 0; i < changedListeners.Length; i++)
                    changedListeners[i].OnAdded(this, component, metadata);
            }

            MugenExtensions.ComponentCollectionOnComponentAdded(this, component, metadata);
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

            if (!MugenExtensions.ComponentCollectionOnComponentRemoving(this, component, metadata))
                return false;

            if (_components != null)
            {
                var changingListeners = _components.GetComponents<IComponentCollectionChangingListener>(metadata);
                for (var i = 0; i < changingListeners.Length; i++)
                {
                    if (!changingListeners[i].OnRemoving(this, component, metadata))
                        return false;
                }
            }


            lock (_items)
            {
                if (!_items.Remove(component))
                    return false;
            }

            UpdateTrackers(component);

            if (_components != null)
            {
                var changedListeners = _components.GetComponents<IComponentCollectionChangedListener>(metadata);
                for (var i = 0; i < changedListeners.Length; i++)
                    changedListeners[i].OnRemoved(this, component, metadata);
            }

            MugenExtensions.ComponentCollectionOnComponentRemoved(this, component, metadata);
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
            var changedListeners = _components.GetComponentsOrDefault<IComponentCollectionChangedListener>(metadata);
            for (var i = 0; i < oldItems.Length; i++)
            {
                var oldItem = oldItems[i];
                for (var j = 0; j < changedListeners.Length; j++)
                    changedListeners[j].OnRemoved(this, oldItem, metadata);
                MugenExtensions.ComponentCollectionOnComponentRemoved(this, oldItem, metadata);
            }

            return true;
        }

        public TComponent[] GetComponents<TComponent>(IReadOnlyMetadataContext? metadata) where TComponent : class
        {
            var componentTrackers = _componentTrackers;
            for (var i = 0; i < componentTrackers.Length; i++)
            {
                if (componentTrackers[i] is ComponentTracker<TComponent> tracker)
                    return tracker.Components;
            }

            return AddNewTracker<TComponent>(componentTrackers, metadata);
        }

        void IHasAddedCallbackComponentOwner.OnComponentAdded(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata)
        {
            if (component is IDecoratorComponentCollectionComponent decorator)
            {
                MugenExtensions.AddOrdered(ref _decorators, decorator, this);
                UpdateTrackers(null, decorator);
            }
        }

        void IHasRemovedCallbackComponentOwner.OnComponentRemoved(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata)
        {
            if (component is IDecoratorComponentCollectionComponent decorator)
            {
                MugenExtensions.Remove(ref _decorators, decorator);
                UpdateTrackers(null, decorator);
            }
        }

        #endregion

        #region Methods

        private TComponent[] AddNewTracker<TComponent>(IComponentTracker[] componentTrackers, IReadOnlyMetadataContext? metadata) where TComponent : class
        {
            var tracker = ComponentTracker<TComponent>.Get(this, metadata);
            Array.Resize(ref componentTrackers, componentTrackers.Length + 1);
            componentTrackers[componentTrackers.Length - 1] = tracker;
            _componentTrackers = componentTrackers;
            return tracker.Components;
        }

        private void UpdateTrackers(object? component, IDecoratorComponentCollectionComponent? decorator = null)
        {
            var componentTrackers = _componentTrackers;
            var newSize = 0;
            for (var i = 0; i < componentTrackers.Length; i++)
            {
                var componentTracker = componentTrackers[i];
                if (!componentTracker.IsComponentSupported(component) && !componentTracker.IsDecoratorSupported(decorator))
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
            bool IsComponentSupported(object? component);

            bool IsDecoratorSupported(IDecoratorComponentCollectionComponent? decorator);
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

            public bool IsComponentSupported(object? component)
            {
                return component is TComponent;
            }

            public bool IsDecoratorSupported(IDecoratorComponentCollectionComponent? decorator)
            {
                return decorator is IDecoratorComponentCollectionComponent<TComponent>;
            }

            #endregion

            #region Methods

            public static ComponentTracker<TComponent> Get(ComponentCollection collection, IReadOnlyMetadataContext? metadata)
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

                    if (collection._decorators.Length != 0 && HasDecorators(collection))
                        return GetComponentTrackerWithDecorators(items, size, collection, metadata);

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

            private static ComponentTracker<TComponent> GetComponentTrackerWithDecorators(List<object> items, int size, ComponentCollection collection, IReadOnlyMetadataContext? metadata)
            {
                List<TComponent> components = new List<TComponent>(size);
                for (var i = 0; i < items.Count; i++)
                {
                    if (items[i] is TComponent c)
                        components.Add(c);
                }

                return new ComponentTracker<TComponent>(Decorate(collection, components, metadata));
            }

            private static bool HasDecorators(ComponentCollection collection)
            {
                var decorators = collection._decorators;
                for (int i = 0; i < decorators.Length; i++)
                {
                    if (decorators[i] is IDecoratorComponentCollectionComponent<TComponent>)
                        return true;
                }

                return false;
            }

            private static TComponent[] Decorate(ComponentCollection collection, List<TComponent> components, IReadOnlyMetadataContext? metadata)
            {
                var decorators = collection._decorators;
                for (var i = 0; i < decorators.Length; i++)
                {
                    if (decorators[i] is IDecoratorComponentCollectionComponent<TComponent> decorator)
                        decorator.Decorate(components, metadata);
                }

                return components.ToArray();
            }

            #endregion
        }

        #endregion
    }
}