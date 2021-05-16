using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Components;
using MugenMvvm.Constants;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Collections.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Internal.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;

// ReSharper disable PossibleMultipleEnumeration

namespace MugenMvvm.Collections.Components
{
    public class FlattenCollectionDecorator : AttachableComponentBase<ICollection>, ICollectionDecorator, IHasPriority
    {
        private readonly Func<object, IEnumerable?> _getNestedCollection;
        private readonly Dictionary<object, FlattenCollectionItem> _collectionItems;
        private ICollectionDecoratorManagerComponent? _decoratorManager;

        public FlattenCollectionDecorator(Func<object, IEnumerable?> getNestedCollection, int priority = CollectionComponentPriority.FlattenCollectionDecorator)
        {
            Should.NotBeNull(getNestedCollection, nameof(getNestedCollection));
            _getNestedCollection = getNestedCollection;
            Priority = priority;
            _collectionItems = new Dictionary<object, FlattenCollectionItem>(InternalEqualityComparer.Reference);
        }

        public int Priority { get; set; }

        protected override void OnAttached(ICollection owner, IReadOnlyMetadataContext? metadata) => _decoratorManager = CollectionDecoratorManager.GetOrAdd(owner);

        protected override void OnDetached(ICollection owner, IReadOnlyMetadataContext? metadata)
        {
            Clear();
            _decoratorManager = null;
        }

        private int UpdateIndexes(int index, int value)
        {
            var result = index;
            foreach (var collectionItem in _collectionItems)
            {
                var item = collectionItem.Value;
                for (var i = 0; i < item.Count; i++)
                {
                    if (item[i] >= index)
                        item[i] += value;
                    else
                        result += item.Size - 1;
                }
            }

            return result;
        }

        private void Clear()
        {
            foreach (var collectionItem in _collectionItems)
                collectionItem.Value.Detach();
            _collectionItems.Clear();
        }

        private bool OnAdded(object? item, ref int index, bool notify = true)
        {
            var originalIndex = index;
            index = UpdateIndexes(index, 1);
            if (item == null)
                return true;

            if (!_collectionItems.TryGetValue(item, out var flattenCollectionItem))
            {
                var nestedCollection = _getNestedCollection(item);
                if (nestedCollection == null)
                    return true;

                flattenCollectionItem = new FlattenCollectionItem(nestedCollection, this);
                _collectionItems[item] = flattenCollectionItem;
            }

            flattenCollectionItem.OnAdded(originalIndex, index, notify);
            return false;
        }

        private bool OnRemoved(object? item, ref int index)
        {
            var originalIndex = index;
            index = UpdateIndexes(index, -1);
            if (!TryGetCollectionItem(item, out var flattenCollectionItem))
                return true;

            if (flattenCollectionItem.OnRemoved(originalIndex - 1, index))
                _collectionItems.Remove(item);
            return false;
        }

        private IEnumerable<object?> Decorate(IEnumerable<object?> items)
        {
            foreach (var item in items)
            {
                if (item != null)
                {
                    var enumerable = _getNestedCollection(item);
                    if (enumerable != null)
                    {
                        foreach (var nestedItem in enumerable.Decorate())
                            yield return nestedItem;
                        continue;
                    }
                }

                yield return item;
            }
        }

        private bool TryGetCollectionItem([NotNullWhen(true)] object? item, [NotNullWhen(true)] out FlattenCollectionItem? value)
        {
            if (item != null && _collectionItems.TryGetValue(item, out value))
                return true;
            value = null;
            return false;
        }

        IEnumerable<object?> ICollectionDecorator.Decorate(ICollection collection, IEnumerable<object?> items) => _decoratorManager == null ? items : Decorate(items);

        bool ICollectionDecorator.OnChanged(ICollection collection, ref object? item, ref int index, ref object? args) =>
            _decoratorManager != null && (item == null || !_collectionItems.ContainsKey(item));

        bool ICollectionDecorator.OnAdded(ICollection collection, ref object? item, ref int index) => _decoratorManager != null && OnAdded(item, ref index);

        bool ICollectionDecorator.OnReplaced(ICollection collection, ref object? oldItem, ref object? newItem, ref int index)
        {
            if (_decoratorManager == null)
                return false;

            var addIndex = index;
            var removed = OnRemoved(oldItem, ref index);
            if (removed)
            {
                if (newItem != null && _getNestedCollection(newItem) != null)
                {
                    _decoratorManager.OnRemoved(collection, this, oldItem, index);
                    removed = false;
                }
            }

            var added = OnAdded(newItem, ref addIndex);
            if (added && removed)
                return true;

            if (removed)
                _decoratorManager.OnRemoved(collection, this, oldItem, index);
            if (added)
                _decoratorManager.OnAdded(collection, this, newItem, index);
            return false;
        }

        bool ICollectionDecorator.OnMoved(ICollection collection, ref object? item, ref int oldIndex, ref int newIndex)
        {
            if (_decoratorManager == null)
                return false;

            if (TryGetCollectionItem(item, out var flattenCollectionItem))
                flattenCollectionItem.Remove(oldIndex);

            var originalNewIndex = newIndex;
            oldIndex = UpdateIndexes(oldIndex, -1);
            newIndex = UpdateIndexes(newIndex, 1);
            if (flattenCollectionItem == null)
                return true;

            flattenCollectionItem.Add(originalNewIndex);
            flattenCollectionItem.OnMoved(oldIndex, newIndex);
            return false;
        }

        bool ICollectionDecorator.OnRemoved(ICollection collection, ref object? item, ref int index) => _decoratorManager != null && OnRemoved(item, ref index);

        bool ICollectionDecorator.OnReset(ICollection collection, ref IEnumerable<object?>? items)
        {
            if (_decoratorManager == null)
                return false;

            Clear();
            if (items != null)
            {
                var index = 0;
                var i = 0;
                foreach (var item in items)
                {
                    OnAdded(item, ref i, false);
                    i = ++index;
                }

                items = Decorate(items);
            }

            return true;
        }

        private sealed class FlattenCollectionItem : List<int>, ICollectionDecoratorListener, IAttachableComponent, ISynchronizationListener
        {
            private readonly FlattenCollectionDecorator _decorator;
            private readonly IEnumerable _collection;
            private List<ActionToken>? _lockTokens;

            public FlattenCollectionItem(IEnumerable collection, FlattenCollectionDecorator decorator) : base(1)
            {
                _decorator = decorator;
                _collection = collection;
            }

            public int Size { get; private set; }

            private ICollectionDecoratorManagerComponent? DecoratorManager => _decorator._decoratorManager;

            public void OnAdded(int originalIndex, int index, bool notify)
            {
                using var _ = MugenExtensions.TryLock(_collection);
                if (notify)
                {
                    Size = 0;
                    foreach (var item in _collection.Decorate())
                    {
                        DecoratorManager!.OnAdded(_decorator.Owner, _decorator, item, index + Size);
                        ++Size;
                    }
                }
                else if (Size == 0)
                    Size = _collection.Decorate().CountEx();

                MugenExtensions.AddOrdered(this, originalIndex, Comparer<int>.Default);
                if (Count == 1 && _collection is IComponentOwner<ICollection> owner)
                    owner.AddComponent(this);
            }

            public bool OnRemoved(int originalIndex, int index)
            {
                using var _ = MugenExtensions.TryLock(_collection);
                foreach (var item in _collection.Decorate())
                    _decorator._decoratorManager!.OnRemoved(_decorator.Owner, _decorator, item, index);

                Remove(originalIndex);
                if (Count == 0)
                {
                    Detach();
                    return true;
                }

                return false;
            }

            public void OnMoved(int oldIndex, int newIndex)
            {
                using var _ = MugenExtensions.TryLock(_collection);
                var removeIndex = 0;
                var addIndex = 0;
                if (oldIndex < newIndex)
                    addIndex += Size - 1;
                foreach (var item in _collection.Decorate())
                {
                    DecoratorManager!.OnRemoved(_decorator.Owner, _decorator, item, oldIndex + removeIndex);
                    DecoratorManager!.OnAdded(_decorator.Owner, _decorator, item, newIndex + addIndex);
                    if (oldIndex > newIndex)
                    {
                        ++removeIndex;
                        ++addIndex;
                    }
                }
            }

            public void Detach()
            {
                if (_collection is IComponentOwner<ICollection> owner)
                    owner.RemoveComponent(this);
            }

            public void OnChanged(ICollection collection, object? item, int index, object? args)
            {
                if (DecoratorManager == null)
                    return;

                for (var i = 0; i < Count; i++)
                    DecoratorManager.OnChanged(_decorator.Owner, _decorator, item, GetIndex(this[i]) + index, args);
            }

            public void OnAdded(ICollection collection, object? item, int index)
            {
                if (DecoratorManager == null)
                    return;

                ++Size;
                for (var i = 0; i < Count; i++)
                    DecoratorManager.OnAdded(_decorator.Owner, _decorator, item, GetIndex(this[i]) + index);
            }

            public void OnReplaced(ICollection collection, object? oldItem, object? newItem, int index)
            {
                if (DecoratorManager == null)
                    return;

                for (var i = 0; i < Count; i++)
                    DecoratorManager.OnReplaced(_decorator.Owner, _decorator, oldItem, newItem, GetIndex(this[i]) + index);
            }

            public void OnMoved(ICollection collection, object? item, int oldIndex, int newIndex)
            {
                if (DecoratorManager == null)
                    return;

                for (var i = 0; i < Count; i++)
                {
                    var originalIndex = GetIndex(this[i]);
                    DecoratorManager.OnMoved(_decorator.Owner, _decorator, item, originalIndex + oldIndex, originalIndex + newIndex);
                }
            }

            public void OnRemoved(ICollection collection, object? item, int index)
            {
                if (DecoratorManager == null)
                    return;

                --Size;
                for (var i = 0; i < Count; i++)
                    DecoratorManager.OnRemoved(_decorator.Owner, _decorator, item, GetIndex(this[i]) + index);
            }

            public void OnReset(ICollection collection, IEnumerable<object?>? items)
            {
                if (DecoratorManager == null)
                    return;

                Size = items.CountEx();
                DecoratorManager.OnReset(_decorator.Owner, _decorator, _decorator.Decorate(DecoratorManager.Decorate(_decorator.Owner, _decorator)));
            }

            private int GetIndex(int originalIndex)
            {
                var result = originalIndex;
                foreach (var collectionItem in _decorator._collectionItems)
                {
                    var item = collectionItem.Value;
                    for (var i = 0; i < item.Count; i++)
                    {
                        if (item[i] < originalIndex)
                            result += item.Size - 1;
                    }
                }

                return result;
            }

            bool IAttachableComponent.OnAttaching(object owner, IReadOnlyMetadataContext? metadata) => true;

            void IAttachableComponent.OnAttached(object owner, IReadOnlyMetadataContext? metadata) => CollectionDecoratorManager.GetOrAdd((IEnumerable) owner);

            void ISynchronizationListener.OnLocking(object target, IReadOnlyMetadataContext? metadata)
            {
                var token = MugenExtensions.TryLock(_decorator.OwnerOptional);
                _lockTokens ??= new List<ActionToken>(2);
                _lockTokens.Add(token);
            }

            void ISynchronizationListener.OnLocked(object target, IReadOnlyMetadataContext? metadata)
            {
            }

            void ISynchronizationListener.OnUnlocking(object target, IReadOnlyMetadataContext? metadata)
            {
            }

            void ISynchronizationListener.OnUnlocked(object target, IReadOnlyMetadataContext? metadata)
            {
                if (_lockTokens == null || _lockTokens.Count == 0)
                    return;

                var token = _lockTokens[_lockTokens.Count - 1];
                _lockTokens.RemoveAt(_lockTokens.Count - 1);
                token.Dispose();
            }
        }
    }
}