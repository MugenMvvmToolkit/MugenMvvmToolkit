using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using MugenMvvm.Extensions;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Components
{
    public sealed class ComponentCollection : IComponentCollection, IComparer<object>, IHasComponentAddedHandler, IHasComponentRemovedHandler, IComparer<IComponentCollectionDecorator>
    {
        #region Fields

        private readonly List<object> _items;
        private IComponentCollection? _components;
        private ComponentTracker[] _componentTrackers;
        private IComponentCollectionDecorator[] _decorators;

        #endregion

        #region Constructors

        public ComponentCollection(object owner)
        {
            Owner = owner;
            _items = new List<object>();
            _componentTrackers = Default.Array<ComponentTracker>();
            _decorators = Default.Array<IComponentCollectionDecorator>();
        }

        #endregion

        #region Properties

        public object Owner { get; }

        public int Count => _items.Count;

        bool IComponentOwner.HasComponents => _components != null && _components.Count != 0;

        IComponentCollection IComponentOwner.Components => _components ?? MugenService.ComponentCollectionManager.EnsureInitialized(ref _components, this);

        #endregion

        #region Implementation of interfaces

        int IComparer<IComponentCollectionDecorator>.Compare([AllowNull] IComponentCollectionDecorator x, [AllowNull] IComponentCollectionDecorator y)
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

        int IComparer<object>.Compare([AllowNull] object x, [AllowNull] object y) => MugenExtensions.GetComponentPriority(y!, Owner).CompareTo(MugenExtensions.GetComponentPriority(x!, Owner));

        public bool TryAdd(object component, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(component, nameof(component));
            lock (_items)
            {
                if (_items.Contains(component))
                    return true;
            }

            if (!ComponentComponentExtensions.OnComponentAdding(this, component, metadata) || !_components.GetOrDefault<IComponentCollectionChangingListener>(metadata).OnAdding(this, component, metadata))
                return false;

            lock (_items)
            {
                MugenExtensions.AddOrdered(_items, component, this);
                UpdateTrackers(component);
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

            if (!ComponentComponentExtensions.OnComponentRemoving(this, component, metadata) || !_components.GetOrDefault<IComponentCollectionChangingListener>(metadata).OnRemoving(this, component, metadata))
                return false;

            lock (_items)
            {
                if (!_items.Remove(component))
                    return false;
                UpdateTrackers(component);
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
                _componentTrackers = Default.Array<ComponentTracker>();
            }

            var changedListeners = _components.GetOrDefault<IComponentCollectionChangedListener>(metadata);
            for (var i = 0; i < oldItems.Length; i++)
            {
                var oldItem = oldItems[i];
                ComponentComponentExtensions.OnComponentRemoved(this, oldItem, metadata);
                changedListeners.OnRemoved(this, oldItem, metadata);
            }
        }

        public TComponent[] Get<TComponent>(IReadOnlyMetadataContext? metadata = null) where TComponent : class
        {
            var componentTrackers = _componentTrackers;
            for (var i = 0; i < componentTrackers.Length; i++)
            {
                if (componentTrackers[i].ComponentType == typeof(TComponent))
                    return (TComponent[]) componentTrackers[i].Components;
            }

            return AddNewTracker<TComponent>(metadata);
        }

        void IHasComponentAddedHandler.OnComponentAdded(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata)
        {
            if (component is IComponentCollectionDecorator decorator)
            {
                lock (_items)
                {
                    MugenExtensions.AddOrdered(ref _decorators, decorator, this);
                    UpdateTrackers(null, decorator);
                }
            }
        }

        void IHasComponentRemovedHandler.OnComponentRemoved(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata)
        {
            if (component is IComponentCollectionDecorator decorator)
            {
                lock (_items)
                {
                    MugenExtensions.Remove(ref _decorators, decorator);
                    UpdateTrackers(null, decorator);
                }
            }
        }

        #endregion

        #region Methods

        private TComponent[] AddNewTracker<TComponent>(IReadOnlyMetadataContext? metadata) where TComponent : class
        {
            lock (_items)
            {
                var tracker = GetComponentTracker<TComponent>(metadata);
                var componentTrackers = _componentTrackers;
                Array.Resize(ref componentTrackers, componentTrackers.Length + 1);
                componentTrackers[componentTrackers.Length - 1] = tracker;
                _componentTrackers = componentTrackers;
                return (TComponent[]) tracker.Components;
            }
        }

        private void UpdateTrackers(object? component, IComponentCollectionDecorator? decorator = null)
        {
            var componentTrackers = _componentTrackers;
            var newSize = 0;
            for (var i = 0; i < componentTrackers.Length; i++)
            {
                var componentTracker = componentTrackers[i];
                if (!componentTracker.IsComponentSupported(component, decorator))
                    componentTrackers[newSize++] = componentTracker;
            }

            if (newSize == 0)
                _componentTrackers = Default.Array<ComponentTracker>();
            else if (newSize != componentTrackers.Length)
                Array.Resize(ref _componentTrackers, newSize);
        }

        private ComponentTracker GetComponentTracker<TComponent>(IReadOnlyMetadataContext? metadata) where TComponent : class
        {
            var size = 0;
            for (var i = 0; i < _items.Count; i++)
            {
                if (_items[i] is TComponent)
                    ++size;
            }

            if (size == 0)
                return ComponentTracker.Get(Default.Array<TComponent>());

            if (_decorators.Length != 0 && _decorators.HasDecorators<TComponent>())
                return GetComponentTrackerWithDecorators<TComponent>(size, metadata);

            var components = new TComponent[size];
            size = 0;
            for (var i = 0; i < _items.Count; i++)
            {
                if (_items[i] is TComponent c)
                    components[size++] = c;
            }

            return ComponentTracker.Get(components);
        }

        private ComponentTracker GetComponentTrackerWithDecorators<TComponent>(int size, IReadOnlyMetadataContext? metadata)
            where TComponent : class
        {
            var components = new List<TComponent>(size);
            for (var i = 0; i < _items.Count; i++)
            {
                if (_items[i] is TComponent c)
                    components.Add(c);
            }

            return ComponentTracker.Get(_decorators.Decorate(this, components, metadata));
        }

        #endregion

        #region Nested types

        [StructLayout(LayoutKind.Auto)]
        private readonly struct ComponentTracker
        {
            #region Fields

            public readonly object[] Components;
            public readonly Type ComponentType;
            public readonly Func<object?, IComponentCollectionDecorator?, bool> IsComponentSupported;

            #endregion

            #region Constructors

            private ComponentTracker(object[] components, Type componentType, Func<object?, IComponentCollectionDecorator?, bool> isComponentSupported)
            {
                Components = components;
                ComponentType = componentType;
                IsComponentSupported = isComponentSupported;
            }

            #endregion

            #region Methods

            public static ComponentTracker Get<TComponent>(TComponent[] components) where TComponent : class =>
                new ComponentTracker(components, typeof(TComponent), (o, decorator) => o is TComponent || decorator is IComponentCollectionDecorator<TComponent>);

            #endregion
        }

        #endregion
    }
}