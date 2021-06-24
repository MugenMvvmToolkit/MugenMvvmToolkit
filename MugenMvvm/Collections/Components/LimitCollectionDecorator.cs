using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MugenMvvm.Constants;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Collections.Components;
using MugenMvvm.Interfaces.Metadata;

// ReSharper disable PossibleMultipleEnumeration

namespace MugenMvvm.Collections.Components
{
    public class LimitCollectionDecorator<T> : CollectionDecoratorBase, IComparer<LimitCollectionDecorator<T>.ItemInfo>
    {
        private const int NotFound = -1;
        private readonly ListInternal<ItemInfo> _items;
        private Func<T, bool>? _condition;
        private int? _limit;

        public LimitCollectionDecorator(int? limit = null, Func<T, bool>? condition = null, int priority = CollectionComponentPriority.LimitDecorator) : base(priority)
        {
            _items = new ListInternal<ItemInfo>(limit.GetValueOrDefault(8));
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

        public int Count => _items.Count;

        private bool HasLimit => Limit != null;

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

            var currentIndex = IndexOf(index);
            var oldSatisfied = currentIndex != NotFound;
            var newSatisfied = IsSatisfied(item);

            if (oldSatisfied == newSatisfied)
            {
                if (oldSatisfied && currentIndex >= Limit)
                    return false;

                index = GetIndex(index);
                return true;
            }

            Replace(decoratorManager, collection, oldSatisfied, newSatisfied, item, item, index);
            return false;
        }

        protected override bool OnAdded(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref object? item, ref int index)
        {
            if (!HasLimit)
                return true;

            if (IsSatisfied(item))
            {
                if (!Add(decoratorManager, (T)item!, index))
                    return false;
            }
            else
                UpdateIndexes(index, 1);

            index = GetIndex(index);
            return true;
        }

        protected override bool OnReplaced(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref object? oldItem,
            ref object? newItem, ref int index)
        {
            if (!HasLimit)
                return true;

            var oldSatisfied = IsSatisfied(oldItem);
            var newSatisfied = IsSatisfied(newItem);

            if (!oldSatisfied && !newSatisfied)
            {
                index = GetIndex(index);
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

            if (oldIndex < Limit && newIndex < Limit || !IsSatisfied(item))
            {
                var toRemove = IndexOf(oldIndex);
                UpdateIndexes(oldIndex + 1, -1);
                UpdateIndexes(newIndex, 1);
                if (toRemove != NotFound)
                {
                    _items.RemoveAt(toRemove);
                    _items.AddOrdered(new ItemInfo((T)item!, newIndex), this);
                }

                oldIndex = GetIndex(oldIndex, oldIndex > newIndex);
                newIndex = GetIndex(newIndex);
                return true;
            }

            var value = (T)item!;
            Remove(decoratorManager, value, oldIndex);
            Add(decoratorManager, value, newIndex);
            return false;
        }

        protected override bool OnRemoved(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref object? item, ref int index)
        {
            if (!HasLimit)
                return true;

            if (IsSatisfied(item))
            {
                if (!Remove(decoratorManager, (T)item!, index))
                    return false;
            }
            else
                UpdateIndexes(index, -1);

            index = GetIndex(index);
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

            using var t = DecoratorManager.TryLock(Owner, this);
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

        private bool IsSatisfied(object? item)
        {
            if (_condition == null)
                return item is T;
            return item is T t && _condition(t);
        }

        private int IndexOf(int index)
        {
            var i = _items.BinarySearch(new ItemInfo(default!, index), this);
            if (i < 0)
                return NotFound;
            return i;
        }

        private void Replace(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, bool oldSatisfied, bool newSatisfied, object? oldItem,
            object? newItem, int index)
        {
            if (!oldSatisfied || Remove(decoratorManager, (T)oldItem!, index))
            {
                if (!oldSatisfied)
                    UpdateIndexes(index, -1);
                decoratorManager.OnRemoved(collection, this, oldItem, GetIndex(index));
            }

            if (!newSatisfied || Add(decoratorManager, (T)newItem!, index))
            {
                if (!newSatisfied)
                    UpdateIndexes(index, 1);
                decoratorManager.OnAdded(collection, this, newItem, GetIndex(index));
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
            var index = 0;
            foreach (var item in items)
            {
                if (IsSatisfied(item))
                    _items.AddOrdered(new ItemInfo((T)item!, index), this);
                ++index;
            }
        }

        private bool Add(ICollectionDecoratorManagerComponent decoratorManager, T item, int index)
        {
            UpdateIndexes(index, 1);
            var newIndex = _items.AddOrdered(new ItemInfo(item, index), this);
            var limit = Limit!.Value;
            if (limit == 0 || newIndex >= limit)
                return false;

            if (_items.Count <= limit)
                return true;

            var oldItem = _items.Items[limit];
            if (index == oldItem.OriginalIndex - 1)
            {
                decoratorManager.OnReplaced(Owner, this, oldItem.Item, item, GetIndex(index));
                return false;
            }

            decoratorManager.OnAdded(Owner, this, item, GetIndex(index));
            decoratorManager.OnRemoved(Owner, this, oldItem.Item, GetIndex(oldItem.OriginalIndex));
            return false;
        }

        private bool Remove(ICollectionDecoratorManagerComponent decoratorManager, T item, int index)
        {
            var indexToRemove = IndexOf(index);
            _items.RemoveAt(indexToRemove);
            UpdateIndexes(index, -1);
            var limit = Limit!.Value;
            if (limit == 0 || indexToRemove >= limit)
                return false;

            if (_items.Count + 1 <= limit)
                return true;

            var oldItem = _items.Items[limit - 1];
            if (index == oldItem.OriginalIndex)
            {
                decoratorManager.OnReplaced(Owner, this, item, oldItem.Item, GetIndex(index));
                return false;
            }

            decoratorManager.OnRemoved(Owner, this, item, GetIndex(index));
            decoratorManager.OnAdded(Owner, this, oldItem.Item, GetIndex(oldItem.OriginalIndex));

            return false;
        }

        private int GetIndex(int index, bool isMove = false)
        {
            if (Limit!.Value > index || Limit!.Value >= _items.Count)
                return index;

            var lastIndex = _items.BinarySearch(new ItemInfo(default!, index), this);
            if (lastIndex == 0)
                return index;
            if (lastIndex < 0)
                lastIndex = ~lastIndex;
            else if (isMove)
                ++lastIndex;
            if (lastIndex <= Limit.Value)
                return index;
            return index - (lastIndex - Limit.Value);
        }

        private void UpdateIndexes(int index, int value)
        {
            var items = _items.Items;
            for (var i = _items.Count - 1; i >= 0; i--)
            {
                if (items[i].OriginalIndex >= index)
                    items[i].OriginalIndex += value;
                else
                    break;
            }
        }

        int IComparer<ItemInfo>.Compare(ItemInfo x, ItemInfo y) => x.OriginalIndex.CompareTo(y.OriginalIndex);

        [StructLayout(LayoutKind.Auto)]
        private struct ItemInfo : IEquatable<ItemInfo>
        {
            // ReSharper disable FieldCanBeMadeReadOnly.Local
            public T Item;

            public int OriginalIndex;
            // ReSharper restore FieldCanBeMadeReadOnly.Local

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ItemInfo(T item, int originalIndex)
            {
                Item = item;
                OriginalIndex = originalIndex;
            }

            public bool Equals(ItemInfo other) => OriginalIndex == other.OriginalIndex;
        }
    }
}