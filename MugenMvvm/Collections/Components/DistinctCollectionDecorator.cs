using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Collections.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

// ReSharper disable PossibleMultipleEnumeration

namespace MugenMvvm.Collections.Components
{
    public sealed class DistinctCollectionDecorator<T, TKey> : CollectionDecoratorBase where TKey : notnull
    {
        private readonly bool _allowNull;
        private readonly Func<T, Optional<TKey>> _getKey;
        private readonly Dictionary<TKey, DistinctItem> _keyMap;
        private readonly FilterDecorator _filterDecorator;
        private IndexMapAwareList<DistinctItem> _indexMap;
        private int? _addingIndex;
        private int? _replacingIndex;
        private object? _replacingItem;

        public DistinctCollectionDecorator(int priority, bool allowNull, Func<T, Optional<TKey>> getKey, IEqualityComparer<TKey>? comparer = null) : base(priority)
        {
            Should.NotBeNull(getKey, nameof(getKey));
            _allowNull = allowNull && TypeChecker.IsNullable<T>();
            _getKey = getKey;
            _indexMap = IndexMapAwareList<DistinctItem>.Get();
            _keyMap = comparer == null ? new Dictionary<TKey, DistinctItem>() : new Dictionary<TKey, DistinctItem>(comparer);
            _filterDecorator = new FilterDecorator(priority - 1, allowNull);
        }

        protected override bool HasAdditionalItems => false;

        protected override void OnAttaching(IReadOnlyObservableCollection owner, IReadOnlyMetadataContext? metadata)
        {
            owner.AddComponent(_filterDecorator);
            base.OnAttaching(owner, metadata);
        }

        protected override void OnDetaching(IReadOnlyObservableCollection owner, IReadOnlyMetadataContext? metadata)
        {
            owner.RemoveComponent(_filterDecorator);
            base.OnDetaching(owner, metadata);
        }

        protected override IEnumerable<object?> Decorate(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection,
            IEnumerable<object?> items) => _addingIndex == null && _replacingIndex == null && _indexMap.Size == 0 ? items : DecorateImpl(items);

        protected override bool OnChanged(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref object? item, ref int index,
            ref object? args)
        {
            if (!IsSatisfied(item, out var itemT))
                return true;

            var newItem = item;
            var binaryIndex = _indexMap.BinarySearch(index);
            TKey? oldKey;
            DistinctItem? distinctItem;
            if (binaryIndex < 0)
            {
                oldKey = default;
                distinctItem = null;
            }
            else
            {
                distinctItem = _indexMap.Indexes[binaryIndex].Value;
                oldKey = distinctItem.Key;
                item = distinctItem;
            }

            var newKey = _getKey(itemT);
            if (binaryIndex >= 0 != newKey.HasNonNullValue || !_keyMap.Comparer.Equals(oldKey!, newKey.GetValueOrDefault()!))
            {
                Replace(decoratorManager, collection, ref newItem, index, distinctItem, binaryIndex, true, newKey);
                item = newItem;
            }

            return true;
        }

        protected override bool OnAdded(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref object? item, ref int index)
        {
            var binaryIndex = _indexMap.BinarySearch(index);
            UpdateIndexesBinary(binaryIndex, 1);
            if (!IsSatisfied(item, out var itemT))
                return true;

            var key = _getKey(itemT);
            if (!key.HasNonNullValue)
                return true;

            var newDistinctItem = new DistinctItem(item, key.Value);
            item = newDistinctItem;
            _indexMap.Add(index, newDistinctItem, binaryIndex);
            if (!_keyMap.TryGetValue(key.Value, out var currentItem))
            {
                _keyMap[key.Value] = newDistinctItem.Head();
                return true;
            }

            if (index <= currentItem.Index)
            {
                _addingIndex = index;
                _keyMap[key.Value] = currentItem.AddFirst(newDistinctItem);
                decoratorManager.OnChanged(collection, this, currentItem, currentItem.Index - 1, null);
                _addingIndex = null;
            }
            else
                currentItem.AddLast(newDistinctItem);

            return true;
        }

        protected override bool OnReplaced(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref object? oldItem,
            ref object? newItem, ref int index)
        {
            var hasOldItemT = IsSatisfied(oldItem, out _);
            var hasNewItemT = IsSatisfied(newItem, out var newItemT);

            if (!hasOldItemT && !hasNewItemT)
                return true;

            var key = hasNewItemT ? _getKey(newItemT!) : default;
            var binaryIndex = _indexMap.BinarySearch(index);
            DistinctItem? distinctItem;
            if (binaryIndex < 0)
                distinctItem = null;
            else
            {
                distinctItem = _indexMap.Indexes[binaryIndex].Value;
                if (hasNewItemT && key.HasNonNullValue && _keyMap.Comparer.Equals(distinctItem.Key, key.Value))
                {
                    distinctItem.Item = newItem;
                    newItem = distinctItem;
                    return true;
                }

                oldItem = distinctItem;
            }

            _replacingIndex = index;
            _replacingItem = oldItem;
            Replace(decoratorManager, collection, ref newItem, index, distinctItem, binaryIndex, hasNewItemT, key);
            _replacingIndex = null;
            _replacingItem = null;
            return true;
        }

        protected override bool OnMoved(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref object? item, ref int oldIndex,
            ref int newIndex)
        {
            if (!_indexMap.Move(oldIndex, newIndex, out var distinctItem))
                return true;

            if (!_keyMap.TryGetValue(distinctItem.Key, out var currentValue))
                return true;

            item = distinctItem;
            if (currentValue.IsSingle)
                return true;

            if (oldIndex > newIndex)
            {
                if (currentValue.Index > distinctItem.Index)
                {
                    _keyMap[distinctItem.Key] = currentValue.MoveToHead(distinctItem);
                    decoratorManager.OnMoved(collection, this, item, oldIndex, newIndex);
                    decoratorManager.OnChanged(collection, this, currentValue, currentValue.Index, null);
                    decoratorManager.OnChanged(collection, this, distinctItem, distinctItem.Index, null);
                    return false;
                }
            }
            else if (currentValue.Index == distinctItem.Index)
            {
                var oldItem = currentValue;
                currentValue = currentValue.Invalidate();
                if (!ReferenceEquals(oldItem, currentValue))
                {
                    _keyMap[distinctItem.Key] = currentValue;
                    decoratorManager.OnMoved(collection, this, item, oldIndex, newIndex);
                    decoratorManager.OnChanged(collection, this, oldItem, oldItem.Index, null);
                    decoratorManager.OnChanged(collection, this, currentValue, currentValue.Index, null);
                    return false;
                }
            }

            return true;
        }

        protected override bool OnRemoved(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref object? item, ref int index)
        {
            var binaryIndex = _indexMap.BinarySearch(index);
            UpdateIndexesBinary(binaryIndex, -1);
            if (binaryIndex < 0)
                return true;

            var oldValue = _indexMap.Indexes[binaryIndex].Value;
            item = oldValue;
            _indexMap.RemoveAt(binaryIndex);
            var currentItem = _keyMap[oldValue.Key];
            if (currentItem.IsSingle)
            {
                _keyMap.Remove(oldValue.Key);
                return true;
            }

            if (currentItem.Index + 1 == index)
            {
                currentItem = currentItem.Remove(currentItem);
                _keyMap[oldValue.Key] = currentItem;
                decoratorManager.OnRemoved(collection, this, item, index);
                decoratorManager.OnChanged(collection, this, currentItem.Item, currentItem.Index, null);
                return false;
            }

            _keyMap[oldValue.Key] = currentItem.Remove(oldValue);
            return true;
        }

        protected override bool OnReset(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref IEnumerable<object?>? items)
        {
            if (items.IsNullOrEmpty())
            {
                _indexMap.Clear();
                _keyMap.Clear();
            }
            else
            {
                var poolSize = _keyMap.Count;
                var poolIndex = 0;
                var pool = ArrayPool<DistinctItem>.Shared.Rent(poolSize);
                try
                {
                    _keyMap.Values.CopyTo(pool, 0);
                    _indexMap.Clear();
                    _keyMap.Clear();

                    var index = 0;
                    foreach (var item in items)
                    {
                        if (IsSatisfied(item, out var itemT))
                        {
                            var key = _getKey(itemT);
                            if (key.HasNonNullValue)
                            {
                                var distinctItem = poolIndex < poolSize ? pool[poolIndex].Recycle(item, key.Value, ref poolIndex) : new DistinctItem(item, key.Value);
                                _indexMap.AddRaw(index, distinctItem);
                                if (_keyMap.TryGetValue(key.Value, out var currentItem))
                                    currentItem.AddLast(distinctItem);
                                else
                                    _keyMap[key.Value] = distinctItem.Head();
                            }
                        }

                        ++index;
                    }

                    items = DecorateImpl(items);
                }
                finally
                {
                    ArrayPool<DistinctItem>.Shared.Return(pool, true);
                }
            }

            return true;
        }

        private void Replace(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref object? newItem,
            int index, DistinctItem? oldDistinctItem, int binaryIndex, bool hasNewItemT, Optional<TKey> key)
        {
            if (binaryIndex >= 0)
            {
                var currentItem = _keyMap[oldDistinctItem!.Key];
                if (!hasNewItemT)
                    _indexMap.RemoveAt(binaryIndex);
                if (currentItem.IsSingle)
                    _keyMap.Remove(oldDistinctItem.Key);
                else
                {
                    var updatedItem = currentItem.Remove(oldDistinctItem);
                    _keyMap[oldDistinctItem.Key] = updatedItem;
                    if (currentItem.Index == index)
                    {
                        decoratorManager.OnChanged(collection, this, oldDistinctItem, index, null);
                        decoratorManager.OnChanged(collection, this, updatedItem, updatedItem.Index, null);
                    }
                }
            }

            if (hasNewItemT && key.HasNonNullValue)
            {
                var newDistinctItem = new DistinctItem(newItem, key.Value);
                newItem = newDistinctItem;
                if (binaryIndex >= 0)
                    _indexMap.Indexes[binaryIndex] = new IndexMapAwareList<DistinctItem>.Entry(index, newDistinctItem);
                else
                    _indexMap.Add(index, newDistinctItem, binaryIndex);
                if (!_keyMap.TryGetValue(key.Value, out var currentItem))
                {
                    _keyMap[key.Value] = newDistinctItem.Head();
                    return;
                }

                if (index <= currentItem.Index)
                {
                    _keyMap[key.Value] = currentItem.AddFirst(newDistinctItem);
                    decoratorManager.OnChanged(collection, this, currentItem, currentItem.Index, null);
                }
                else
                    currentItem.AddLast(newDistinctItem);
            }
        }

        private IEnumerable<object?> DecorateImpl(IEnumerable<object?> items)
        {
            var index = -1;
            var itemIndex = 0;
            foreach (var item in items)
            {
                ++index;
                if (index == _addingIndex)
                    continue;

                if (index == _replacingIndex)
                {
                    yield return _replacingItem;
                    continue;
                }

                if (itemIndex < _indexMap.Size)
                {
                    var entry = _indexMap.Indexes[itemIndex];
                    if (entry.Index == index)
                    {
                        ++itemIndex;
                        yield return entry.Value;
                        continue;
                    }
                }

                yield return item;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsSatisfied(object? item, [MaybeNullWhen(false)] out T itemT) => item.TryCast(_allowNull, out itemT);

        private void UpdateIndexesBinary(int binarySearchIndex, int value)
        {
            if (_indexMap.Size == 0)
                return;
            if (binarySearchIndex < 0)
                binarySearchIndex = ~binarySearchIndex;
            for (var i = binarySearchIndex; i < _indexMap.Size; i++)
            {
                ref var entry = ref _indexMap.Indexes[i];
                entry._index += value;
                entry.Value.Index += value;
            }
        }

        private sealed class DistinctItem : IndexMapAware
        {
            public object? Item;
            public TKey Key;
            public bool IsVisible;
            private DistinctItem? _prev;
            private DistinctItem? _next;

            public DistinctItem(object? item, TKey key)
            {
                Item = item;
                Key = key;
            }

            public bool IsSingle => ReferenceEquals(_prev, this);

            public DistinctItem Head()
            {
                IsVisible = true;
                _next = this;
                _prev = this;
                return this;
            }

            public DistinctItem MoveToHead(DistinctItem item)
            {
                if (!ReferenceEquals(item, this))
                {
                    IsVisible = false;
                    item.IsVisible = true;
                    return Remove(item).AddFirst(item);
                }

                return item;
            }

            public DistinctItem AddFirst(DistinctItem item)
            {
                IsVisible = false;
                item.IsVisible = true;
                InsertBefore(this, item);
                return item;
            }

            public void AddLast(DistinctItem item) => InsertBefore(this, item);

            public DistinctItem Remove(DistinctItem item)
            {
                var head = Remove(this, item)!;
                if (ReferenceEquals(item, this))
                {
                    IsVisible = false;
                    head.IsVisible = true;
                }

                return head;
            }

            public DistinctItem Invalidate()
            {
                var node = this;
                DistinctItem? currentItem = null;
                do
                {
                    if (currentItem == null || currentItem.Index > node.Index)
                        currentItem = node;
                    node = node._next!;
                } while (node != this);

                return MoveToHead(currentItem);
            }

            public DistinctItem Recycle(object? item, TKey key, ref int index)
            {
                var recycledItem = _prev!;
                if (IsSingle)
                    ++index;
                else
                    Remove(recycledItem);
                recycledItem._next = null;
                recycledItem._prev = null;
                recycledItem.Item = item;
                recycledItem.Key = key;
                recycledItem.IsVisible = false;
                return recycledItem;
            }

#if DEBUG
            public override string ToString() => Item?.ToString()!;
#endif

            private static void InsertBefore(DistinctItem node, DistinctItem newNode)
            {
                newNode._next = node;
                newNode._prev = node._prev;
                node._prev!._next = newNode;
                node._prev = newNode;
            }

            private static DistinctItem? Remove(DistinctItem head, DistinctItem node)
            {
                node.IsVisible = false;
                if (node._next == node)
                    return null;
                node._next!._prev = node._prev;
                node._prev!._next = node._next;
                return head == node ? node._next : head;
            }
        }

        private sealed class FilterDecorator : FilterCollectionDecorator<DistinctItem>
        {
            public FilterDecorator(int priority, bool allowNull) : base(priority, allowNull, (item, _) => item.IsVisible)
            {
            }

            protected override bool HasItemDecorator => true;

            protected override bool OnChanged(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref object? item, ref int index,
                ref object? args)
            {
                var result = base.OnChanged(decoratorManager, collection, ref item, ref index, ref args);
                if (result && item is DistinctItem distinctItem)
                    item = distinctItem.Item;
                return result;
            }

            protected override bool OnAdded(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref object? item, ref int index)
            {
                var result = base.OnAdded(decoratorManager, collection, ref item, ref index);
                if (result && item is DistinctItem distinctItem)
                    item = distinctItem.Item;
                return result;
            }

            protected override bool OnReplaced(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref object? oldItem,
                ref object? newItem, ref int index)
            {
                var result = base.OnReplaced(decoratorManager, collection, ref oldItem, ref newItem, ref index);
                if (result)
                {
                    if (oldItem is DistinctItem oldDistinctItem)
                        oldItem = oldDistinctItem.Item;
                    if (newItem is DistinctItem newDistinctItem)
                        newItem = newDistinctItem.Item;
                    return true;
                }

                return false;
            }

            protected override bool OnMoved(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref object? item, ref int oldIndex,
                ref int newIndex)
            {
                var result = base.OnMoved(decoratorManager, collection, ref item, ref oldIndex, ref newIndex);
                if (result && item is DistinctItem distinctItem)
                    item = distinctItem.Item;
                return result;
            }

            protected override bool OnRemoved(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref object? item, ref int index)
            {
                var result = base.OnRemoved(decoratorManager, collection, ref item, ref index);
                if (result && item is DistinctItem distinctItem)
                    item = distinctItem.Item;
                return result;
            }

            protected override object? Decorate(object? item)
            {
                if (item is DistinctItem distinctItem)
                    return distinctItem.Item;
                return item;
            }
        }
    }
}