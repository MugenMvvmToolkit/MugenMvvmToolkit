using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using MugenMvvm.Constants;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Collections.Components;
using MugenMvvm.Interfaces.Metadata;

// ReSharper disable PossibleMultipleEnumeration

namespace MugenMvvm.Collections.Components
{
    public class LimitCollectionDecorator<T> : CollectionDecoratorBase where T : notnull
    {
        private IndexMapList<object?> _items;
        private Func<T, bool>? _condition;
        private int? _limit;

        public LimitCollectionDecorator(int? limit = null, Func<T, bool>? condition = null, int priority = CollectionComponentPriority.LimitDecorator) : base(priority)
        {
            _items = IndexMapList<object?>.Get();
            _condition = condition;
            _limit = limit;
            Priority = priority;
        }

        protected override bool HasAdditionalItems => false;

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
            var newSatisfied = IsSatisfied(item);

            if (oldSatisfied == newSatisfied)
            {
                if (oldSatisfied && currentIndex >= Limit)
                    return false;

                index = GetIndex(index, currentIndex);
                return true;
            }

            Replace(decoratorManager, collection, oldSatisfied, newSatisfied, item, item, index);
            return false;
        }

        protected override bool OnAdded(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref object? item, ref int index)
        {
            if (!HasLimit)
                return true;

            int? currentIndex;
            if (IsSatisfied(item))
            {
                if (!Add(decoratorManager, item, index, out currentIndex))
                    return false;
            }
            else
            {
                currentIndex = null;
                _items.UpdateIndexes(index, 1);
            }

            index = GetIndex(index, currentIndex);
            return true;
        }

        protected override bool OnReplaced(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref object? oldItem,
            ref object? newItem, ref int index)
        {
            if (!HasLimit)
                return true;

            var oldSatisfied = IsSatisfied(oldItem);
            var newSatisfied = IsSatisfied(newItem);

            if (oldSatisfied == newSatisfied)
            {
                int? currentIndex;
                if (oldSatisfied)
                {
                    currentIndex = _items.BinarySearch(index);
                    _items.Indexes[currentIndex.Value].Value = newItem;
                    if (currentIndex.Value >= Limit.Value)
                        return false;
                }
                else
                    currentIndex = null;

                index = GetIndex(index, currentIndex);
                return true;
            }

            Replace(decoratorManager, collection, oldSatisfied, newSatisfied, oldItem, newItem, index);
            return false;
        }

        protected override bool OnMoved(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref object? item, ref int oldIndex,
            ref int newIndex)
        {
            if (!HasLimit)
                return true;

            var limit = Limit.Value;
            int? oldBinaryIndex = null;
            if (oldIndex < limit && newIndex < limit || !IsSatisfied(item) || _items.Size <= limit)
            {
                int? newBinaryIndex = null;
                _items.Move(oldIndex, newIndex, out _, ref oldBinaryIndex, ref newBinaryIndex);
                oldIndex = GetIndex(oldIndex, oldBinaryIndex, oldIndex > newIndex);
                newIndex = GetIndex(newIndex, null);
                return oldIndex != newIndex;
            }

            if (oldIndex > limit && newIndex > limit)
            {
                oldBinaryIndex = _items.BinarySearch(oldIndex);
                int? newBinaryIndex = _items.BinarySearch(newIndex);
                if (oldBinaryIndex.Value > limit && newBinaryIndex.Value > limit)
                {
                    _items.Move(oldIndex, newIndex, out _, ref oldBinaryIndex, ref newBinaryIndex);
                    return false;
                }
            }

            Remove(decoratorManager, item, oldIndex, ref oldBinaryIndex);
            Add(decoratorManager, item, newIndex, out _);
            return false;
        }

        protected override bool OnRemoved(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref object? item, ref int index)
        {
            if (!HasLimit)
                return true;

            int? currentIndex = null;
            if (IsSatisfied(item))
            {
                if (!Remove(decoratorManager, item, index, ref currentIndex))
                    return false;
            }
            else
                _items.UpdateIndexes(index, -1);

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

        private void Update(Func<T, bool>? condition, int? limit)
        {
            if (DecoratorManager == null)
            {
                _condition = condition;
                _limit = limit;
                return;
            }

            using var _ = Owner.Lock();
            _condition = condition;
            _limit = limit;
            _items.Clear();

            var items = DecoratorManager.Decorate(Owner, this);
            if (HasLimit)
            {
                UpdateItems(items);
                items = Decorate(items);
            }

            DecoratorManager.OnReset(Owner, this, items);
        }

        private void Replace(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, bool oldSatisfied, bool newSatisfied, object? oldItem,
            object? newItem, int index)
        {
            int? currentIndex = null;
            if (!oldSatisfied || Remove(decoratorManager, oldItem, index, ref currentIndex))
            {
                if (!oldSatisfied)
                    _items.UpdateIndexes(index, -1);
                decoratorManager.OnRemoved(collection, this, oldItem, GetIndex(index, currentIndex));
                currentIndex = null;
            }

            if (!newSatisfied || Add(decoratorManager, newItem, index, out currentIndex))
            {
                if (!newSatisfied)
                    _items.UpdateIndexes(index, 1);
                decoratorManager.OnAdded(collection, this, newItem, GetIndex(index, currentIndex));
            }
        }

        private IEnumerable<object?> Decorate(IEnumerable<object?> items)
        {
            var count = 0;
            foreach (var item in items)
            {
                if (IsSatisfied(item) && ++count > Limit)
                    continue;

                yield return item;
            }
        }

        private void UpdateItems(IEnumerable<object?> items)
        {
            if (items is IReadOnlyCollection<object?> c)
                _items.EnsureCapacity(c.Count);

            var index = 0;
            foreach (var item in items)
            {
                if (IsSatisfied(item))
                    _items.AddRaw(index, item);
                ++index;
            }
        }

        private bool Add(ICollectionDecoratorManagerComponent decoratorManager, object? item, int index, out int? binarySearchIndex)
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
                decoratorManager.OnReplaced(Owner, this, oldItem, item, GetIndex(index, binarySearchIndex));
                return false;
            }

            decoratorManager.OnAdded(Owner, this, item, GetIndex(index, binarySearchIndex));
            decoratorManager.OnRemoved(Owner, this, oldItem, GetIndex(oldIndex, limit));
            return false;
        }

        private bool Remove(ICollectionDecoratorManagerComponent decoratorManager, object? item, int index, [NotNull] ref int? removedIndex)
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
                decoratorManager.OnReplaced(Owner, this, item, oldItem, GetIndex(index, removedIndex));
                return false;
            }

            decoratorManager.OnRemoved(Owner, this, item, GetIndex(index, removedIndex));
            decoratorManager.OnAdded(Owner, this, oldItem, GetIndex(oldIndex, limit - 1));

            return false;
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