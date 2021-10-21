using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Collections.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;
using MugenMvvm.Metadata;

// ReSharper disable PossibleMultipleEnumeration

namespace MugenMvvm.Collections.Components
{
    public sealed class DistinctCollectionDecorator<T, TKey> : CollectionDecoratorBase where TKey : notnull
    {
        private readonly bool _allowNull;
        private readonly Func<T, Optional<TKey>> _getKey;
        private readonly Dictionary<TKey, DistinctGroup> _keyMap;
        private readonly ICollectionDecorator _filterDecorator;
        private IndexMapAwareList<DistinctItem> _indexMap;
        private int? _addingIndex;
        private int? _replacingIndex;
        private object? _replacingItem;
#if !NET5_0
        private List<TKey>? _resetCache;
#endif

        public DistinctCollectionDecorator(int priority, bool allowNull, Func<T, Optional<TKey>> getKey, IEqualityComparer<TKey>? comparer = null) : base(priority)
        {
            Should.NotBeNull(getKey, nameof(getKey));
            _allowNull = allowNull && TypeChecker.IsNullable<T>();
            _getKey = getKey;
            _indexMap = IndexMapAwareList<DistinctItem>.Get();
            _keyMap = comparer == null ? new Dictionary<TKey, DistinctGroup>() : new Dictionary<TKey, DistinctGroup>(comparer);
            _filterDecorator = new FilterCollectionDecorator<T>(priority - 1, allowNull, Filter);
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
            IEnumerable<object?> items) => _addingIndex == null && _replacingIndex == null ? items : DecorateImpl(items);

        protected override bool OnChanged(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref object? item, ref int index,
            ref object? args)
        {
            if (!IsSatisfied(item, out var itemT))
                return true;

            var binaryIndex = _indexMap.BinarySearch(index);
            var oldKey = binaryIndex < 0 ? default : _indexMap.Indexes[binaryIndex].Value.Key;
            var newKey = _getKey(itemT);
            if (binaryIndex >= 0 != newKey.HasNonNullValue || !_keyMap.Comparer.Equals(oldKey!, newKey.GetValueOrDefault()!))
                Replace(decoratorManager, collection, item, index, binaryIndex, true, newKey);

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
            _indexMap.Add(index, newDistinctItem, binaryIndex);
            if (!_keyMap.TryGetValue(key.Value, out var distinctInfo))
            {
                _keyMap[key.Value] = new DistinctGroup(newDistinctItem, newDistinctItem);
                return true;
            }

            if (index <= distinctInfo.Current.Index)
            {
                _addingIndex = index;
                _keyMap[key.Value] = distinctInfo.Add(newDistinctItem, newDistinctItem);
                decoratorManager.OnChanged(collection, this, distinctInfo.Current.Item, distinctInfo.Current.Index - 1, CollectionMetadata.FalseFilterArgs);
                _addingIndex = null;
            }
            else
                _keyMap[key.Value] = distinctInfo.Add(newDistinctItem, null);

            return true;
        }

        protected override bool OnReplaced(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref object? oldItem,
            ref object? newItem, ref int index)
        {
            var hasOldItemT = IsSatisfied(oldItem, out _);
            var hasNewItemT = IsSatisfied(newItem, out var newItemT);

            if (!hasOldItemT && !hasNewItemT)
                return true;

            _replacingIndex = index;
            _replacingItem = oldItem;
            Replace(decoratorManager, collection, newItem, index, _indexMap.BinarySearch(index), hasNewItemT, hasNewItemT ? _getKey(newItemT!) : default);
            _replacingIndex = null;
            _replacingItem = null;
            return true;
        }

        protected override bool OnMoved(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref object? item, ref int oldIndex,
            ref int newIndex)
        {
            if (!_indexMap.Move(oldIndex, newIndex, out var distinctItem))
                return true;

            if (!_keyMap.TryGetValue(distinctItem.Key, out var currentValue) || currentValue.Count == 1)
                return true;

            var oldItem = currentValue.Current;
            if (oldIndex > newIndex)
            {
                if (oldItem.Index > distinctItem.Index)
                {
                    _keyMap[distinctItem.Key] = currentValue.Update(distinctItem);
                    decoratorManager.OnMoved(collection, this, item, oldIndex, newIndex);
                    decoratorManager.OnChanged(collection, this, oldItem.Item, oldItem.Index, CollectionMetadata.FalseFilterArgs);
                    decoratorManager.OnChanged(collection, this, distinctItem.Item, distinctItem.Index, CollectionMetadata.TrueFilterArgs);
                    return false;
                }
            }
            else
            {
                if (oldItem.Index == distinctItem.Index)
                {
                    currentValue = currentValue.Invalidate();
                    _keyMap[distinctItem.Key] = currentValue;
                    decoratorManager.OnMoved(collection, this, item, oldIndex, newIndex);
                    decoratorManager.OnChanged(collection, this, oldItem.Item, oldItem.Index, CollectionMetadata.FalseFilterArgs);
                    decoratorManager.OnChanged(collection, this, currentValue.Current.Item, currentValue.Current.Index, CollectionMetadata.TrueFilterArgs);
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
            _indexMap.RemoveAt(binaryIndex);
            var distinctGroup = _keyMap[oldValue.Key];
            if (distinctGroup.Count == 1)
            {
                _keyMap.Remove(oldValue.Key);
                return true;
            }

            if (distinctGroup.Current.Index + 1 == index)
            {
                distinctGroup = distinctGroup.Remove();
                _keyMap[oldValue.Key] = distinctGroup;
                decoratorManager.OnRemoved(collection, this, item, index);
                decoratorManager.OnChanged(collection, this, distinctGroup.Current.Item, distinctGroup.Current.Index, CollectionMetadata.TrueFilterArgs);
                return false;
            }

            _keyMap[oldValue.Key] = distinctGroup.Remove(oldValue);
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
                _indexMap.Clear();
#if !NET5_0
                _resetCache?.Clear();
#endif
                foreach (var pair in _keyMap)
                {
                    if (pair.Value.Clear())
                        continue;
#if NET5_0
                    _keyMap.Remove(pair.Key);
#else
                    _resetCache ??= new List<TKey>();
                    _resetCache.Add(pair.Key);
#endif
                }
#if !NET5_0
                RemoveUnusedKeys();
#endif

                var index = 0;
                foreach (var item in items)
                {
                    if (IsSatisfied(item, out var itemT))
                    {
                        var key = _getKey(itemT);
                        if (key.HasNonNullValue)
                        {
                            var distinctItem = new DistinctItem(item, key.Value);
                            _indexMap.AddRaw(index, distinctItem);
                            if (_keyMap.TryGetValue(key.Value, out var group))
                            {
                                if (group.Count == 0)
                                    _keyMap[key.Value] = group.Add(distinctItem, distinctItem);
                                else
                                    _keyMap[key.Value] = group.Add(distinctItem, null);
                            }
                            else
                                _keyMap[key.Value] = new DistinctGroup(distinctItem, distinctItem);
                        }
                    }

                    ++index;
                }


                foreach (var pair in _keyMap)
                {
                    if (pair.Value.Count != 0)
                        continue;

#if NET5_0
                    _keyMap.Remove(pair.Key);
#else
                    _resetCache ??= new List<TKey>();
                    _resetCache.Add(pair.Key);
#endif
                }

#if !NET5_0
                RemoveUnusedKeys();
#endif
            }

            return true;
        }

        private bool Filter(T item, int index)
        {
            var binarySearch = _indexMap.BinarySearch(index);
            return binarySearch >= 0 && _indexMap.Indexes[binarySearch].Value.IsVisible;
        }

        private void Replace(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, object? newItem, int index, int binaryIndex,
            bool hasNewItemT, Optional<TKey> key)
        {
            if (binaryIndex >= 0)
            {
                var oldDistinctItem = _indexMap.Indexes[binaryIndex].Value;
                var oldItemGroup = _keyMap[oldDistinctItem.Key];
                if (!hasNewItemT)
                    _indexMap.RemoveAt(binaryIndex);
                if (oldItemGroup.Count == 1)
                    _keyMap.Remove(oldDistinctItem.Key);
                else
                {
                    var updatedGroup = oldItemGroup.Remove(oldDistinctItem);
                    _keyMap[oldDistinctItem.Key] = updatedGroup;
                    if (oldItemGroup.Current.Index == index)
                    {
                        decoratorManager.OnChanged(collection, this, oldDistinctItem.Item, index, CollectionMetadata.FalseFilterArgs);
                        decoratorManager.OnChanged(collection, this, updatedGroup.Current.Item, updatedGroup.Current.Index, CollectionMetadata.TrueFilterArgs);
                    }
                }
            }

            if (hasNewItemT && key.HasNonNullValue)
            {
                var newDistinctItem = new DistinctItem(newItem, key.Value);
                if (binaryIndex >= 0)
                    _indexMap.Indexes[binaryIndex] = new IndexMapAwareList<DistinctItem>.Entry(index, newDistinctItem);
                else
                    _indexMap.Add(index, newDistinctItem, binaryIndex);
                if (!_keyMap.TryGetValue(key.Value, out var distinctInfo))
                {
                    _keyMap[key.Value] = new DistinctGroup(newDistinctItem, newDistinctItem);
                    return;
                }

                if (index <= distinctInfo.Current.Index)
                {
                    _keyMap[key.Value] = distinctInfo.Add(newDistinctItem, newDistinctItem);
                    decoratorManager.OnChanged(collection, this, distinctInfo.Current.Item, distinctInfo.Current.Index, CollectionMetadata.FalseFilterArgs);
                }
                else
                    _keyMap[key.Value] = distinctInfo.Add(newDistinctItem, null);
            }
        }

        private IEnumerable<object?> DecorateImpl(IEnumerable<object?> items)
        {
            int index = 0;
            foreach (var item in items)
            {
                if (index != _addingIndex)
                {
                    if (index == _replacingIndex)
                        yield return _replacingItem;
                    else
                        yield return item;
                }

                ++index;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsSatisfied(object? item, [NotNullWhen(true)] out T? itemT) => item.TryCast(_allowNull, out itemT);

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

#if !NET5_0
        private void RemoveUnusedKeys()
        {
            if (_resetCache != null && _resetCache.Count != 0)
            {
                for (var i = 0; i < _resetCache.Count; i++)
                    _keyMap.Remove(_resetCache[i]);
                _resetCache.Clear();
            }
        }
#endif

        [StructLayout(LayoutKind.Auto)]
        private readonly struct DistinctGroup
        {
            public readonly DistinctItem Current;
            private readonly object _item;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public DistinctGroup(DistinctItem current, object item)
            {
                _item = item;
                Current = current;
                current.IsVisible = true;
            }

            public int Count
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get
                {
                    if (_item is List<DistinctItem> items)
                        return items.Count;
                    return 1;
                }
            }

            public DistinctGroup Add(DistinctItem item, DistinctItem? current)
            {
                if (current == null)
                    current = Current;
                else
                    Current.IsVisible = false;
                if (_item is List<DistinctItem> list)
                {
                    list.Add(item);
                    return new DistinctGroup(current, list);
                }

                return new DistinctGroup(current, new List<DistinctItem>(2) {(DistinctItem) _item, item});
            }

            public bool Clear()
            {
                if (_item is List<DistinctItem> items)
                {
                    items.Clear();
                    return true;
                }

                return false;
            }

            public DistinctGroup Remove(DistinctItem item)
            {
                ((List<DistinctItem>) _item).Remove(item);
                if (item.Index == Current.Index)
                    return Invalidate();
                return this;
            }

            public DistinctGroup Remove()
            {
                Current.IsVisible = false;
                ((List<DistinctItem>) _item).Remove(Current);
                return Invalidate();
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public DistinctGroup Update(DistinctItem item) => new(item, _item);

            public DistinctGroup Invalidate()
            {
                var items = (List<DistinctItem>) _item;
                DistinctItem? currentItem = null;
                for (var i = 0; i < items.Count; i++)
                {
                    if (currentItem == null || currentItem.Index > items[i].Index)
                        currentItem = items[i];
                }

                return new DistinctGroup(currentItem!, items);
            }
        }

        private sealed class DistinctItem : IndexMapAware
        {
            public readonly object? Item;
            public readonly TKey Key;
            public bool IsVisible;

            public DistinctItem(object? item, TKey key)
            {
                Item = item;
                Key = key;
            }

#if DEBUG
            public override string ToString() => Item?.ToString()!;
#endif
        }
    }
}