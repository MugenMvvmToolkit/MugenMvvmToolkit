using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using MugenMvvm.Collections;
using MugenMvvm.Extensions;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Components
{
    public sealed class ComponentCollection : IComponentCollection, IComparer<object>, IHasComponentAddedHandler, IHasComponentRemovedHandler,
        IComparer<IComponentCollectionDecoratorBase>
    {
        private readonly IComponentCollectionManager? _componentCollectionManager;
        private readonly List<object> _items;
        private IComponentCollection? _components;
        private ComponentTracker[] _componentTrackers;
        private IComponentCollectionDecoratorBase[] _decorators;

        public ComponentCollection(object owner, IComponentCollectionManager? componentCollectionManager = null)
        {
            _componentCollectionManager = componentCollectionManager;
            Owner = owner;
            _items = new List<object>();
            _componentTrackers = Array.Empty<ComponentTracker>();
            _decorators = Array.Empty<IComponentCollectionDecoratorBase>();
        }

        public object Owner { get; }

        public int Count => _items.Count;

        bool IComponentOwner.HasComponents => _components != null && _components.Count != 0;

        IComponentCollection IComponentOwner.Components => _components ?? _componentCollectionManager.EnsureInitialized(ref _components, this);

        public bool TryAdd(object component, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(component, nameof(component));
            lock (_items)
            {
                if (_items.Contains(component))
                    return true;
            }

            if (!ComponentComponentExtensions.OnComponentAdding(this, component, metadata) ||
                _components != null && !_components.Get<IComponentCollectionChangingListener>(metadata).OnAdding(this, component, metadata))
                return false;

            lock (_items)
            {
                MugenExtensions.AddOrdered(_items, component, this);
                UpdateTrackers(component, null, metadata);
            }

            ComponentComponentExtensions.OnComponentAdded(this, component, metadata);
            _components?.Get<IComponentCollectionChangedListener>(metadata).OnAdded(this, component, metadata);
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

            if (!ComponentComponentExtensions.OnComponentRemoving(this, component, metadata) ||
                _components != null && !_components.Get<IComponentCollectionChangingListener>(metadata).OnRemoving(this, component, metadata))
                return false;

            lock (_items)
            {
                if (!_items.Remove(component))
                    return false;
                UpdateTrackers(component, null, metadata);
            }

            ComponentComponentExtensions.OnComponentRemoved(this, component, metadata);
            _components?.Get<IComponentCollectionChangedListener>().OnRemoved(this, component, metadata);
            return true;
        }

        public void Clear(IReadOnlyMetadataContext? metadata = null)
        {
            var oldItems = Get<object>(metadata);
            lock (_items)
            {
                _items.Clear();
                _componentTrackers = Array.Empty<ComponentTracker>();
            }

            var changedListeners = _components == null ? default : _components.Get<IComponentCollectionChangedListener>(metadata);
            foreach (var oldItem in oldItems)
            {
                ComponentComponentExtensions.OnComponentRemoved(this, oldItem, metadata);
                changedListeners.OnRemoved(this, oldItem, metadata);
            }
        }

        public void Invalidate(object component, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(component, nameof(component));
            lock (_items)
            {
                if (!_items.Contains(component))
                    return;

                _items.Sort(this);
                UpdateTrackers(component, null, metadata);
            }
        }

        public ItemOrArray<T> Get<T>(IReadOnlyMetadataContext? metadata = null) where T : class
        {
            foreach (var tracker in _componentTrackers)
            {
                if (tracker.ComponentType == typeof(T))
                    return ItemOrArray.FromRawValue<T>(tracker.Components);
            }

            return AddNewTracker<T>(metadata);
        }

        private ItemOrArray<TComponent> AddNewTracker<TComponent>(IReadOnlyMetadataContext? metadata) where TComponent : class
        {
            lock (_items)
            {
                var tracker = GetComponentTracker<TComponent>(metadata);
                var componentTrackers = _componentTrackers;
                Array.Resize(ref componentTrackers, componentTrackers.Length + 1);
                componentTrackers[componentTrackers.Length - 1] = tracker;
                _componentTrackers = componentTrackers;
                return ItemOrArray.FromRawValue<TComponent>(tracker.Components);
            }
        }

        private void UpdateTrackers(object? component, IComponentCollectionDecoratorBase? decorator, IReadOnlyMetadataContext? metadata)
        {
            var componentTrackers = _componentTrackers;
            var newSize = 0;
            for (var i = 0; i < componentTrackers.Length; i++)
            {
                var componentTracker = componentTrackers[i];
                if (!componentTracker.IsComponentSupported(component, decorator, metadata))
                    componentTrackers[newSize++] = componentTracker;
            }

            if (newSize == 0)
                _componentTrackers = Array.Empty<ComponentTracker>();
            else if (newSize != componentTrackers.Length)
                Array.Resize(ref _componentTrackers, newSize);
        }

        private ComponentTracker GetComponentTracker<TComponent>(IReadOnlyMetadataContext? metadata) where TComponent : class
        {
            if (_decorators.Length != 0 && _decorators.HasDecorators<TComponent>(metadata))
                return GetComponentTrackerWithDecorators<TComponent>(metadata);

            var size = 0;
            for (var i = 0; i < _items.Count; i++)
            {
                if (_items[i] is TComponent)
                    ++size;
            }

            if (size == 0)
                return ComponentTracker.Get<TComponent>(default);

            var components = ItemOrArray.Get<TComponent>(size);
            size = 0;
            for (var i = 0; i < _items.Count; i++)
            {
                if (_items[i] is TComponent c)
                    components.SetAt(size++, c);
            }

            return ComponentTracker.Get(components);
        }

        private ComponentTracker GetComponentTrackerWithDecorators<TComponent>(IReadOnlyMetadataContext? metadata)
            where TComponent : class
        {
            var components = new ItemOrListEditor<TComponent>();
            for (var i = 0; i < _items.Count; i++)
            {
                if (_items[i] is TComponent c)
                    components.Add(c);
            }

            _decorators.Decorate(this, ref components, metadata);
            return ComponentTracker.Get(components.ToItemOrArray());
        }

        int IComparer<IComponentCollectionDecoratorBase>.Compare(IComponentCollectionDecoratorBase? x, IComponentCollectionDecoratorBase? y)
        {
            var result = MugenExtensions.GetComponentPriority(x!, this).CompareTo(MugenExtensions.GetComponentPriority(y!, this));
            if (result == 0)
            {
                lock (_items)
                {
                    var xIndex = _items.IndexOf(x!);
                    var yIndex = _items.IndexOf(y!);
                    return yIndex.CompareTo(xIndex);
                }
            }

            return result;
        }

        int IComparer<object>.Compare(object? x, object? y) => MugenExtensions.GetComponentPriority(y!, Owner).CompareTo(MugenExtensions.GetComponentPriority(x!, Owner));

        void IHasComponentAddedHandler.OnComponentAdded(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata)
        {
            if (component is IComponentCollectionDecoratorBase decorator)
            {
                lock (_items)
                {
                    MugenExtensions.AddOrdered(ref _decorators, decorator, this);
                    UpdateTrackers(null, decorator, metadata);
                }
            }
        }

        void IHasComponentRemovedHandler.OnComponentRemoved(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata)
        {
            if (component is IComponentCollectionDecoratorBase decorator)
            {
                lock (_items)
                {
                    MugenExtensions.Remove(ref _decorators, decorator);
                    UpdateTrackers(null, decorator, metadata);
                }
            }
        }

        [StructLayout(LayoutKind.Auto)]
        private readonly struct ComponentTracker
        {
            public readonly object? Components;
            public readonly Type ComponentType;
            public readonly Func<object?, IComponentCollectionDecoratorBase?, IReadOnlyMetadataContext?, bool> IsComponentSupported;

            private ComponentTracker(object? components, Type componentType,
                Func<object?, IComponentCollectionDecoratorBase?, IReadOnlyMetadataContext?, bool> isComponentSupported)
            {
                Components = components;
                ComponentType = componentType;
                IsComponentSupported = isComponentSupported;
            }

            public static ComponentTracker Get<T>(ItemOrArray<T> components) where T : class =>
                new(components.GetRawValue(), typeof(T),
                    (o, decorator, m) => o is T || decorator is IComponentCollectionDecorator<T> || decorator is IComponentCollectionDecorator d && d.CanDecorate<T>(m));
        }
    }
}