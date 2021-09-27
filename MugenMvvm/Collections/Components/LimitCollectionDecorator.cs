using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Collections.Components;
using MugenMvvm.Interfaces.Metadata;

// ReSharper disable PossibleMultipleEnumeration

namespace MugenMvvm.Collections.Components
{
    public sealed class LimitCollectionDecorator<T> : CollectionDecoratorBase where T : notnull
    {
        private IndexMapList<object?> _items;
        private Func<T, bool>? _condition;
        private int? _limit;
        private bool _isAdding;
        private bool _isRemoving;

        public LimitCollectionDecorator(int priority, int? limit = null, Func<T, bool>? condition = null) : base(priority)
        {
            _items = IndexMapList<object?>.Get();
            _condition = condition;
            _limit = limit;
            Priority = priority;
        }

        public Func<T, bool>? Condition
        {
            get => _condition;
            set
            {
                if (_condition != value)
                    Update(value, _limit);
            }
        }

        public int? Limit
        {
            get => _limit;
            set
            {
                if (_limit != value)
                    Update(_condition, value);
            }
        }

        public int Count => _items.Size;

        protected override bool HasAdditionalItems => false;

        [MemberNotNullWhen(true, nameof(Limit))]
        private bool HasLimit => Limit != null && Limit.Value != int.MaxValue;

        protected override void OnDetached(IReadOnlyObservableCollection owner, IReadOnlyMetadataContext? metadata)
        {
            base.OnDetached(owner, metadata);
            _items.Clear();
        }

        protected override IEnumerable<object?> Decorate(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection,
            IEnumerable<object?> items) => HasLimit ? Decorate(items) : items;

        protected override bool OnChanged(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref object? item, ref int index,
            ref object? args)
        {
            if (!HasLimit)
                return true;

            var currentIndex = _items.BinarySearch(index);
            var oldSatisfied = currentIndex >= 0;
            if (oldSatisfied == IsSatisfied(item))
            {
                if (oldSatisfied && currentIndex >= Limit)
                    return false;

                index = GetIndex(index, currentIndex);
                return true;
            }

            Replace(decoratorManager, collection, oldSatisfied, item, item, index, currentIndex);
            return false;
        }

        protected override bool OnAdded(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref object? item, ref int index)
        {
            if (!HasLimit)
                return true;

            int? currentIndex;
            if (IsSatisfied(item))
            {
                if (!Add(decoratorManager, collection, item, index, out currentIndex))
                    return false;
            }
            else
            {
                currentIndex = _items.BinarySearch(index);
                _items.UpdateIndexesBinary(currentIndex.Value, 1);
            }

            index = GetIndex(index, currentIndex);
            return true;
        }

        protected override bool OnReplaced(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref object? oldItem,
            ref object? newItem, ref int index)
        {
            if (!HasLimit)
                return true;

            var oldBinaryIndex = _items.BinarySearch(index);
            var oldSatisfied = oldBinaryIndex >= 0;
            if (oldSatisfied == IsSatisfied(newItem))
            {
                if (oldSatisfied)
                {
                    _items.Indexes[oldBinaryIndex].Value = newItem;
                    if (oldBinaryIndex >= Limit.Value)
                        return false;
                }

                index = GetIndex(index, oldBinaryIndex);
                return true;
            }

            Replace(decoratorManager, collection, oldSatisfied, oldItem, newItem, index, oldBinaryIndex);
            return false;
        }

        protected override bool OnMoved(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref object? item, ref int oldIndex,
            ref int newIndex)
        {
            if (!HasLimit)
                return true;

            var limit = Limit.Value;
            int? oldBinaryIndex = _items.BinarySearch(oldIndex);
            int? newBinaryIndex = null;
            if (oldIndex < limit && newIndex < limit || oldBinaryIndex.Value < 0 || _items.Size <= limit)
            {
                _items.Move(oldIndex, newIndex, out _, ref oldBinaryIndex, ref newBinaryIndex);
                oldIndex = GetIndex(oldIndex, oldBinaryIndex, oldIndex > newIndex);
                newIndex = GetIndex(newIndex, null);
                return oldIndex != newIndex;
            }

            newBinaryIndex = _items.BinarySearch(newIndex);
            _items.Move(oldIndex, newIndex, out _, ref oldBinaryIndex, ref newBinaryIndex);

            if (limit == 0 || GetIndex(oldBinaryIndex.Value) >= limit && GetIndex(newBinaryIndex.Value) >= limit)
                return false;

            if (GetIndex(oldBinaryIndex.Value) < limit && GetIndex(newBinaryIndex.Value) < limit)
            {
                oldIndex = GetIndex(oldIndex, oldBinaryIndex, oldIndex > newIndex);
                newIndex = GetIndex(newIndex, null);
                return oldIndex != newIndex;
            }

            if (oldIndex > newIndex)
            {
                _isAdding = true;
                decoratorManager.OnAdded(collection, this, item, newIndex);
                _isAdding = false;
                decoratorManager.OnRemoved(collection, this, _items.Indexes[limit].Value, _items.Indexes[limit].Index);
            }
            else
            {
                limit -= 1;
                _isRemoving = true;
                decoratorManager.OnRemoved(collection, this, item, oldIndex);
                _isRemoving = false;
                decoratorManager.OnAdded(collection, this, _items.Indexes[limit].Value, _items.Indexes[limit].Index);
            }

            return false;
        }

        protected override bool OnRemoved(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref object? item, ref int index)
        {
            if (!HasLimit)
                return true;

            int? currentIndex = _items.BinarySearch(index);
            if (currentIndex.Value >= 0)
            {
                if (!Remove(decoratorManager, collection, item, index, ref currentIndex))
                    return false;
            }
            else
                _items.UpdateIndexesBinary(currentIndex.Value, -1);

            index = GetIndex(index, currentIndex);
            return true;
        }

        protected override bool OnReset(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref IEnumerable<object?>? items)
        {
            if (!HasLimit)
                return true;

            _items.Clear();
            if (items != null)
            {
                UpdateItems(items);
                items = Decorate(items);
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetIndex(int binarySearchIndex) => binarySearchIndex < 0 ? ~binarySearchIndex : binarySearchIndex;

        private bool Add(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, object? item, int index, out int? binarySearchIndex)
        {
            binarySearchIndex = _items.BinarySearch(index);
            _items.UpdateIndexesBinary(binarySearchIndex.Value, 1);
            var newIndex = _items.Add(index, item, binarySearchIndex.Value);
            var limit = Limit!.Value;
            if (limit == 0 || newIndex >= limit)
                return false;

            if (_items.Size <= limit)
                return true;

            var oldIndex = _items.Indexes[limit].Index;
            var oldItem = _items.Indexes[limit].Value;
            if (index == oldIndex - 1)
            {
                decoratorManager.OnReplaced(collection, this, oldItem, item, GetIndex(index, binarySearchIndex));
                return false;
            }

            _isAdding = true;
            decoratorManager.OnAdded(collection, this, item, GetIndex(index, binarySearchIndex));
            _isAdding = false;
            decoratorManager.OnRemoved(collection, this, oldItem, GetIndex(oldIndex, limit));
            return false;
        }

        private bool Remove(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, object? item, int index,
            [NotNull] ref int? removedIndex)
        {
            removedIndex ??= _items.BinarySearch(index);
            _items.UpdateIndexesBinary(removedIndex.Value, -1);
            _items.RemoveAt(removedIndex.Value);
            var limit = Limit!.Value;
            if (limit == 0 || removedIndex.Value >= limit)
                return false;

            if (_items.Size + 1 <= limit)
                return true;

            var oldIndex = _items.Indexes[limit - 1].Index;
            var oldItem = _items.Indexes[limit - 1].Value;
            if (index == oldIndex)
            {
                decoratorManager.OnReplaced(collection, this, item, oldItem, GetIndex(index, removedIndex));
                return false;
            }

            _isRemoving = true;
            decoratorManager.OnRemoved(collection, this, item, GetIndex(index, removedIndex));
            _isRemoving = false;
            decoratorManager.OnAdded(collection, this, oldItem, GetIndex(oldIndex, limit - 1));

            return false;
        }

        private void Replace(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, bool oldSatisfied, object? oldItem, object? newItem,
            int index, int binaryIndex)
        {
            var limit = Limit!.Value;
            if (oldSatisfied)
            {
                _items.RemoveAt(binaryIndex);
                if (binaryIndex < limit)
                {
                    _isRemoving = true;
                    decoratorManager.OnReplaced(collection, this, oldItem, newItem, index);
                    _isRemoving = false;
                    if (_items.Size >= limit)
                        decoratorManager.OnAdded(collection, this, _items.Indexes[limit - 1].Value, _items.Indexes[limit - 1].Index);
                    return;
                }

                decoratorManager.OnAdded(collection, this, newItem, GetIndex(index, binaryIndex));
                return;
            }

            if (_items.Add(index, newItem, binaryIndex) >= limit)
            {
                decoratorManager.OnRemoved(collection, this, oldItem, GetIndex(index, binaryIndex));
                return;
            }

            _isAdding = true;
            decoratorManager.OnReplaced(collection, this, oldItem, newItem, index);
            _isAdding = false;
            if (_items.Size > limit)
                decoratorManager.OnRemoved(collection, this, _items.Indexes[limit].Value, _items.Indexes[limit].Index);
        }

        private void Update(Func<T, bool>? condition, int? limit)
        {
            var decoratorManager = DecoratorManager;
            var owner = OwnerOptional;
            if (decoratorManager == null || owner == null)
            {
                _condition = condition;
                _limit = limit;
                return;
            }

            using var _ = owner.Lock();
            _condition = condition;
            _limit = limit;
            if (DecoratorManager == null)
                return;

            _items.Clear();
            var items = decoratorManager.Decorate(owner, this);
            if (HasLimit)
            {
                UpdateItems(items);
                items = Decorate(items);
            }

            decoratorManager.OnReset(owner, this, items);
        }

        private IEnumerable<object?> Decorate(IEnumerable<object?> items)
        {
            if (_items.Size == 0 || Limit == null)
                return items;
            return DecorateImpl(items, Limit.Value);
        }

        private IEnumerable<object?> DecorateImpl(IEnumerable<object?> items, int limit)
        {
            if (_isAdding)
                ++limit;
            else if (_isRemoving)
                --limit;
            var count = 0;
            var index = 0;
            var itemIndex = 0;
            foreach (var item in items)
            {
                if (itemIndex < _items.Size)
                {
                    var entry = _items.Indexes[itemIndex];
                    if (entry.Index == index)
                    {
                        ++count;
                        ++index;
                        ++itemIndex;
                        if (count <= limit)
                            yield return item;
                        continue;
                    }
                }

                ++index;
                yield return item;
            }
        }

        private void UpdateItems(IEnumerable<object?> items)
        {
            if (items.IsNullOrEmpty())
                return;

            var index = 0;
            foreach (var item in items)
            {
                if (IsSatisfied(item))
                    _items.AddRaw(index, item);
                ++index;
            }
        }

        private int GetIndex(int index, int? binaryIndex, bool isMove = false)
        {
            if (Limit!.Value > index || Limit!.Value >= _items.Size)
                return index;

            var lastIndex = binaryIndex ?? _items.BinarySearch(index);
            if (lastIndex < 0)
                lastIndex = ~lastIndex;
            else if (isMove)
                ++lastIndex;
            if (lastIndex <= Limit.Value)
                return index;
            return index - (lastIndex - Limit.Value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsSatisfied(object? item)
        {
            if (_condition == null)
                return item is T;
            return item is T t && _condition(t);
        }
    }
}