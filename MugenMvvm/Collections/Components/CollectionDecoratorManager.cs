using System.Collections.Generic;
using System.Linq;
using System.Threading;
using MugenMvvm.Attributes;
using MugenMvvm.Components;
using MugenMvvm.Constants;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Collections.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Models.Components;
using MugenMvvm.Internal;

// ReSharper disable PossibleMultipleEnumeration

namespace MugenMvvm.Collections.Components
{
    public sealed class CollectionDecoratorManager<T> : AttachableComponentBase<IReadOnlyObservableCollection>, ICollectionDecoratorManagerComponent, ICollectionChangedListener<T>,
        IComponentCollectionChangedListener, IDisposableComponent<IReadOnlyObservableCollection>, IHasPriority
    {
        private const int InvalidDecoratorIndex = -1;
        private readonly ComponentTracker _tracker;

        private int _batchCount;
        private ItemOrArray<ICollectionDecorator> _decorators;
        private ItemOrArray<ICollectionDecoratorListener> _listeners;
        private ItemOrArray<ICollectionBatchUpdateListener> _batchListeners;

        [Preserve]
        public CollectionDecoratorManager() : this(CollectionComponentPriority.DecoratorManager)
        {
        }

        public CollectionDecoratorManager(int priority)
        {
            Priority = priority;
            _tracker = new ComponentTracker {Priority = priority + 1};
            _tracker.AddListener<ICollectionDecorator, CollectionDecoratorManager<T>>((array, manager, _) => manager._decorators = array, this);
            _tracker.AddListener<ICollectionDecoratorListener, CollectionDecoratorManager<T>>((array, manager, _) => manager._listeners = array, this);
            _tracker.AddListener<ICollectionBatchUpdateListener, CollectionDecoratorManager<T>>((array, manager, _) => manager._batchListeners = array, this);
        }

        public int Priority { get; }

        public IEnumerable<object?> Decorate(IReadOnlyObservableCollection collection, ICollectionDecorator? decorator = null)
        {
            using var _ = decorator == null ? collection.TryLock() : default;
            var items = collection.AsEnumerable();
            var decorators = GetDecorators(decorator, out var index, true);
            if (index == InvalidDecoratorIndex)
                yield break;

            for (var i = 0; i < index; i++)
                items = decorators[i].Decorate(collection, items);

            foreach (var item in items)
                yield return item;
        }

        public void RaiseItemChanged(IReadOnlyObservableCollection collection, object item, object? args)
        {
            using var l = collection.TryLock();
            var indexes = new ItemOrListEditor<int>();
            var items = collection.AsEnumerable();
            items.FindAllIndexOf(item, ref indexes);

            if (indexes.Count != 0)
            {
                OnChanged(collection, _decorators, 0, item, indexes, args);
                return;
            }

            var decorators = _decorators;
            for (var i = 0; i < decorators.Count; i++)
            {
                var decorator = decorators[i];
                items = decorator.Decorate(collection, items);
                FindAllIndexOf(collection, decorator, items, item, ref indexes);
                if (indexes.Count != 0)
                {
                    OnChanged(collection, decorators, i, item, indexes, args);
                    return;
                }
            }
        }

        private void OnChanged(IReadOnlyObservableCollection collection, ItemOrArray<ICollectionDecorator> decorators, int startIndex, object? item,
            ItemOrIReadOnlyList<int> indexes, object? args)
        {
            using var token = BatchUpdate();
            foreach (var currentIndex in indexes)
            {
                int index = currentIndex;
                for (var i = startIndex; i < decorators.Count; i++)
                {
                    if (!decorators[i].OnChanged(collection, ref item, ref index, ref args))
                        return;
                }

                _listeners.OnChanged(collection, item, index, args);
            }
        }

        void ICollectionDecoratorManagerComponent.OnChanged(IReadOnlyObservableCollection collection, ICollectionDecorator? decorator, object? item, int index, object? args)
        {
            var decorators = GetDecorators(decorator, out var startIndex);
            if (startIndex == InvalidDecoratorIndex)
                return;

            using var token = BatchUpdate();
            for (var i = startIndex; i < decorators.Count; i++)
            {
                if (!decorators[i].OnChanged(collection, ref item, ref index, ref args))
                    return;
            }

            _listeners.OnChanged(collection, item, index, args);
        }

        public void OnAdded(IReadOnlyObservableCollection collection, ICollectionDecorator? decorator, object? item, int index)
        {
            var decorators = GetDecorators(decorator, out var startIndex);
            if (startIndex == InvalidDecoratorIndex)
                return;

            using var token = BatchUpdate();
            for (var i = startIndex; i < decorators.Count; i++)
            {
                if (!decorators[i].OnAdded(collection, ref item, ref index))
                    return;
            }

            _listeners.OnAdded(collection, item, index);
        }

        public void OnReplaced(IReadOnlyObservableCollection collection, ICollectionDecorator? decorator, object? oldItem, object? newItem, int index)
        {
            var decorators = GetDecorators(decorator, out var startIndex);
            if (startIndex == InvalidDecoratorIndex)
                return;

            using var token = BatchUpdate();
            for (var i = startIndex; i < decorators.Count; i++)
            {
                if (!decorators[i].OnReplaced(collection, ref oldItem, ref newItem, ref index))
                    return;
            }

            _listeners.OnReplaced(collection, oldItem, newItem, index);
        }

        public void OnMoved(IReadOnlyObservableCollection collection, ICollectionDecorator? decorator, object? item, int oldIndex, int newIndex)
        {
            if (oldIndex == newIndex)
                return;

            var decorators = GetDecorators(decorator, out var startIndex);
            if (startIndex == InvalidDecoratorIndex)
                return;

            using var token = BatchUpdate();
            for (var i = startIndex; i < decorators.Count; i++)
            {
                if (!decorators[i].OnMoved(collection, ref item, ref oldIndex, ref newIndex) || oldIndex == newIndex)
                    return;
            }

            _listeners.OnMoved(collection, item, oldIndex, newIndex);
        }

        public void OnRemoved(IReadOnlyObservableCollection collection, ICollectionDecorator? decorator, object? item, int index)
        {
            var decorators = GetDecorators(decorator, out var startIndex);
            if (startIndex == InvalidDecoratorIndex)
                return;

            using var token = BatchUpdate();
            for (var i = startIndex; i < decorators.Count; i++)
            {
                if (!decorators[i].OnRemoved(collection, ref item, ref index))
                    return;
            }

            _listeners.OnRemoved(collection, item, index);
        }

        public void OnReset(IReadOnlyObservableCollection collection, ICollectionDecorator? decorator, IEnumerable<object?>? items)
        {
            var decorators = GetDecorators(decorator, out var startIndex);
            if (startIndex == InvalidDecoratorIndex)
                return;

            using var token = BatchUpdate();
            for (var i = startIndex; i < decorators.Count; i++)
            {
                if (!decorators[i].OnReset(collection, ref items))
                    return;
            }

            _listeners.OnReset(collection, items);
        }

        protected override void OnAttached(IReadOnlyObservableCollection owner, IReadOnlyMetadataContext? metadata)
        {
            base.OnAttached(owner, metadata);
            _tracker.Attach(owner);
            owner.Components.AddComponent(this);
            Reset(null);
        }

        protected override void OnDetached(IReadOnlyObservableCollection owner, IReadOnlyMetadataContext? metadata)
        {
            base.OnDetached(owner, metadata);
            _tracker.Detach(owner);
            owner.Components.RemoveComponent(this);
        }

        private ItemOrArray<ICollectionDecorator> GetDecorators(ICollectionDecorator? decorator, out int index, bool isLengthDefault = false)
        {
            var components = _decorators;
            index = isLengthDefault ? components.Count : 0;
            if (decorator == null)
                return components;

            var found = false;
            for (var i = 0; i < components.Count; i++)
            {
                if (components[i] == decorator)
                {
                    found = true;
                    index = i;
                    if (!isLengthDefault)
                        ++index;
                    break;
                }
            }

            if (!found)
            {
                index = InvalidDecoratorIndex;
                return default;
            }

            return components;
        }

        private static void FindAllIndexOf(IReadOnlyObservableCollection collection, ICollectionDecorator decorator, IEnumerable<object?> items, object item,
            ref ItemOrListEditor<int> indexes)
        {
            if (!decorator.HasAdditionalItems)
                return;

            indexes.Clear();
            if (!decorator.TryGetIndexes(collection, items, item, ref indexes))
                items.FindAllIndexOf(item, ref indexes);
        }

        private void Reset(object? component)
        {
            if (component != null && component is not ICollectionDecorator)
                return;

            var collection = OwnerOptional;
            if (collection == null)
                return;

            using var _ = collection.TryLock();
            OnReset(collection, null, collection.AsEnumerable());
        }

        private ActionToken BatchUpdate()
        {
            var collection = OwnerOptional;
            if (collection == null || _batchListeners.Count == 0)
                return default;

            if (Interlocked.Increment(ref _batchCount) == 1)
                _batchListeners.OnBeginBatchUpdate(collection, BatchUpdateType.Decorators);
            return ActionToken.FromDelegate((@this, col) => ((CollectionDecoratorManager<T>) @this!).EndBatchUpdate((IReadOnlyObservableCollection) col!), this, collection);
        }

        private void EndBatchUpdate(IReadOnlyObservableCollection collection)
        {
            if (Interlocked.Decrement(ref _batchCount) == 0)
                _batchListeners.OnEndBatchUpdate(collection, BatchUpdateType.Decorators);
        }

        void ICollectionChangedListener<T>.OnAdded(IReadOnlyObservableCollection<T> collection, T item, int index) => OnAdded(collection, null, item, index);

        void ICollectionChangedListener<T>.OnReplaced(IReadOnlyObservableCollection<T> collection, T oldItem, T newItem, int index) =>
            OnReplaced(collection, null, oldItem, newItem, index);

        void ICollectionChangedListener<T>.OnMoved(IReadOnlyObservableCollection<T> collection, T item, int oldIndex, int newIndex) =>
            OnMoved(collection, null, item, oldIndex, newIndex);

        void ICollectionChangedListener<T>.OnRemoved(IReadOnlyObservableCollection<T> collection, T item, int index) =>
            OnRemoved(collection, null, item, index);

        void ICollectionChangedListener<T>.OnReset(IReadOnlyObservableCollection<T> collection, IReadOnlyCollection<T>? items)
        {
            if (items == null)
                OnReset(collection, null, null);
            else
                OnReset(collection, null, items as IEnumerable<object?> ?? items.Cast<object>());
        }

        ActionToken ICollectionDecoratorManagerComponent.BatchUpdate(IReadOnlyObservableCollection collection, ICollectionDecorator? decorator) => BatchUpdate();

        void IComponentCollectionChangedListener.OnAdded(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata) => Reset(component);

        void IComponentCollectionChangedListener.OnRemoved(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata) => Reset(component);

        void IDisposableComponent<IReadOnlyObservableCollection>.Dispose(IReadOnlyObservableCollection owner, IReadOnlyMetadataContext? metadata)
        {
            using var _ = owner.TryLock();
            owner.RemoveComponents<ICollectionDecoratorManagerComponent>(metadata);
            owner.RemoveComponents<ICollectionDecoratorListener>(metadata);
            owner.ClearComponents(metadata);
        }
    }
}