using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using MugenMvvm.Collections;
using MugenMvvm.Extensions;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models.Components;
using MugenMvvm.Internal;

namespace MugenMvvm.Components
{
    public sealed class ComponentCollection : IComponentCollection, IHasComponentAddedHandler, IHasComponentRemovedHandler, IComparer<object>,
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

        public object? TryAdd<T>(T state, Func<IComponentCollection, T, IReadOnlyMetadataContext?, object?> tryGetComponent, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(tryGetComponent, nameof(tryGetComponent));
            using (Lock())
            {
                var component = tryGetComponent(this, state, metadata);
                if (component == null)
                    return null;
                var added = TryAddInternal(component, metadata);

                if (added.GetValueOrDefault())
                {
                    RaiseAdded(component, metadata);
                    return component;
                }

                if (added == null)
                    return component;
                return null;
            }
        }

        public bool TryAdd(object component, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(component, nameof(component));
            using (Lock())
            {
                var added = TryAddInternal(component, metadata);
                if (added.GetValueOrDefault())
                    RaiseAdded(component, metadata);
                return added.GetValueOrDefault(true);
            }
        }

        public bool Remove(object component, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(component, nameof(component));
            using (Lock())
            {
                if (!_items.Contains(component))
                    return false;

                if (!ComponentComponentExtensions.CanRemove(this, component, metadata) ||
                    _components != null && !_components.Get<IConditionComponentCollectionComponent>(metadata).CanRemove(this, component, metadata))
                    return false;

                ComponentComponentExtensions.OnComponentRemoving(this, component, metadata);
                _components?.Get<IComponentCollectionChangingListener>(metadata).OnRemoving(this, component, metadata);
                _items.Remove(component);
                UpdateTrackers(component, null, metadata);

                ComponentComponentExtensions.OnComponentRemoved(this, component, metadata);
                _components?.Get<IComponentCollectionChangedListener>(metadata).OnRemoved(this, component, metadata);
                return true;
            }
        }

        public void Clear(IReadOnlyMetadataContext? metadata = null)
        {
            if (Count == 0)
                return;

            using (Lock())
            {
                var conditionComponents = _components == null ? default : _components.Get<IConditionComponentCollectionComponent>(metadata);
                var changingListeners = _components == null ? default : _components.Get<IComponentCollectionChangingListener>(metadata);
                var ignoredIndexes = new ItemOrListEditor<int>();

                var components = Get<object>(metadata);
                if (components.Count == 0)
                    return;

                for (var i = 0; i < components.Count; i++)
                {
                    var component = components[i];
                    if (!ComponentComponentExtensions.CanRemove(this, component, metadata) || !conditionComponents.CanRemove(this, component, metadata))
                        ignoredIndexes.Add(i);
                }

                if (ignoredIndexes.Count == components.Count)
                    return;

                if (ignoredIndexes.Count == 0)
                {
                    for (var i = 0; i < _items.Count; i++)
                    {
                        var component = _items[i];
                        ComponentComponentExtensions.OnComponentRemoving(this, component, metadata);
                        changingListeners.OnRemoving(this, component, metadata);
                    }

                    _items.Clear();
                    _componentTrackers = Array.Empty<ComponentTracker>();
                }
                else
                {
                    for (var i = 0; i < components.Count; i++)
                    {
                        if (ignoredIndexes.Contains(i))
                            continue;

                        var component = components[i];
                        if (!_items.Contains(component))
                            continue;

                        ComponentComponentExtensions.OnComponentRemoving(this, component, metadata);
                        changingListeners.OnRemoving(this, component, metadata);
                        _items.Remove(component);
                        UpdateTrackers(component, null, metadata);
                    }
                }

                var changedListeners = _components == null ? default : _components.Get<IComponentCollectionChangedListener>(metadata);
                for (var i = 0; i < components.Count; i++)
                {
                    if (ignoredIndexes.Contains(i))
                        continue;

                    var component = components[i];
                    ComponentComponentExtensions.OnComponentRemoved(this, component, metadata);
                    changedListeners.OnRemoved(this, component, metadata);
                }
            }
        }

        public ItemOrArray<T> Get<T>(IReadOnlyMetadataContext? metadata = null) where T : class
        {
            foreach (var tracker in _componentTrackers)
            {
                if (tracker.ComponentType == typeof(T))
                    return ItemOrArray.FromRawValueFixedArray<T>(tracker.Components);
            }

            return AddNewTracker<T>(metadata);
        }

        public void Invalidate(object? component, IReadOnlyMetadataContext? metadata = null)
        {
            if (component == null)
                return;

            using (Lock())
            {
                if (!_items.Contains(component))
                    return;

                _items.Sort(this);
                UpdateTrackers(component, null, metadata);

                _components?.Get<IHasCacheComponent<IComponentCollection>>(metadata).Invalidate(this, component, metadata);
                (Owner as IHasComponentChangedHandler)?.OnComponentChanged(this, component, metadata);
            }
        }

        private bool? TryAddInternal(object component, IReadOnlyMetadataContext? metadata)
        {
            if (_items.Contains(component))
                return null;

            if (!ComponentComponentExtensions.CanAdd(this, component, metadata) ||
                _components != null && !_components.Get<IConditionComponentCollectionComponent>(metadata).CanAdd(this, component, metadata))
                return false;

            ComponentComponentExtensions.OnComponentAdding(this, component, metadata);
            _components?.Get<IComponentCollectionChangingListener>(metadata).OnAdding(this, component, metadata);
            MugenExtensions.AddOrdered(_items, component, this);
            UpdateTrackers(component, null, metadata);
            return true;
        }

        private void RaiseAdded(object component, IReadOnlyMetadataContext? metadata)
        {
            ComponentComponentExtensions.OnComponentAdded(this, component, metadata);
            _components?.Get<IComponentCollectionChangedListener>(metadata).OnAdded(this, component, metadata);
        }

        private ItemOrArray<TComponent> AddNewTracker<TComponent>(IReadOnlyMetadataContext? metadata) where TComponent : class
        {
            using (Lock())
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
            var components = new ItemOrListEditor<TComponent>(2);
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
            var result = MugenExtensions.GetComponentPriority(x!).CompareTo(MugenExtensions.GetComponentPriority(y!));
            if (result == 0)
                return _items.IndexOf(y!).CompareTo(_items.IndexOf(x!));
            return result;
        }

        int IComparer<object>.Compare(object? x, object? y) => MugenExtensions.GetComponentPriority(y!).CompareTo(MugenExtensions.GetComponentPriority(x!));

        void IHasComponentAddedHandler.OnComponentAdded(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata)
        {
            if (component is IComponentCollectionDecoratorBase decorator)
            {
                using (Lock())
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
                using (Lock())
                {
                    MugenExtensions.Remove(ref _decorators, decorator);
                    UpdateTrackers(null, decorator, metadata);
                }
            }
        }

        private ActionToken Lock()
        {
            if (Owner is ISynchronizable synchronizable)
                return synchronizable.Lock();

            var lockTaken = false;
            try
            {
                Monitor.Enter(_items, ref lockTaken);
                return ActionToken.FromDelegate((o, _) => Monitor.Exit(o!), _items);
            }
            catch
            {
                if (lockTaken)
                    Monitor.Exit(_items);
                throw;
            }
        }

        [StructLayout(LayoutKind.Auto)]
        private readonly struct ComponentTracker
        {
            public readonly object? Components;
            public readonly Type ComponentType;
            public readonly Func<object?, IComponentCollectionDecoratorBase?, IReadOnlyMetadataContext?, bool> IsComponentSupported;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private ComponentTracker(object? components, Type componentType,
                Func<object?, IComponentCollectionDecoratorBase?, IReadOnlyMetadataContext?, bool> isComponentSupported)
            {
                Components = components;
                ComponentType = componentType;
                IsComponentSupported = isComponentSupported;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static ComponentTracker Get<T>(ItemOrArray<T> components) where T : class =>
                new(components.GetRawValue(), typeof(T),
                    (o, decorator, m) => o is T || decorator is IComponentCollectionDecorator<T> || decorator is IComponentCollectionDecorator d && d.CanDecorate<T>(m));
        }
    }
}