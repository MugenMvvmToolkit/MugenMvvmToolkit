using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MugenMvvm.Components;
using MugenMvvm.Constants;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Collections.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

// ReSharper disable PossibleMultipleEnumeration

namespace MugenMvvm.Collections.Components
{
    public class LimitCollectionDecorator<T> : AttachableComponentBase<ICollection>, ICollectionDecorator, IHasPriority, IComparer<LimitCollectionDecorator<T>.ItemInfo>
    {
        private const int NotFound = -1;
        private readonly List<ItemInfo> _items;
        private Func<T, bool>? _condition;
        private int? _limit;

        public LimitCollectionDecorator(int? limit = null, Func<T, bool>? condition = null, int priority = CollectionComponentPriority.LimitDecorator)
        {
            _items = new List<ItemInfo>();
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

        public int Priority { get; set; }

        protected ICollectionDecoratorManagerComponent? DecoratorManager { get; private set; }

        private bool HasCondition => Limit != null && DecoratorManager != null;

        protected override void OnAttached(ICollection owner, IReadOnlyMetadataContext? metadata) => DecoratorManager = CollectionDecoratorManager.GetOrAdd(owner);

        protected override void OnDetached(ICollection owner, IReadOnlyMetadataContext? metadata)
        {
            _items.Clear();
            DecoratorManager = null;
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
            if (HasCondition)
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

        private void Replace(ICollection collection, bool oldSatisfied, bool newSatisfied, object? oldItem, object? newItem, int index)
        {
            using var t = DecoratorManager!.BatchUpdate(collection, this);
            if (!oldSatisfied || Remove((T) oldItem!, index))
            {
                if (!oldSatisfied)
                    UpdateIndexes(index, -1);
                DecoratorManager!.OnRemoved(collection, this, oldItem, GetIndex(index));
            }

            if (!newSatisfied || Add((T) newItem!, index))
            {
                if (!newSatisfied)
                    UpdateIndexes(index, 1);
                DecoratorManager!.OnAdded(collection, this, newItem, GetIndex(index));
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
                    MugenExtensions.AddOrdered(_items, new ItemInfo((T) item!, index), this);
                ++index;
            }
        }

        private bool Add(T item, int index)
        {
            UpdateIndexes(index, 1);
            var newIndex = MugenExtensions.AddOrdered(_items, new ItemInfo(item, index), this);
            var limit = Limit!.Value;
            if (limit == 0 || newIndex >= limit)
                return false;

            if (_items.Count <= limit)
                return true;

            var oldItem = _items[limit];
            if (index == oldItem.OriginalIndex - 1)
            {
                DecoratorManager!.OnReplaced(Owner, this, oldItem.Item, item, GetIndex(index));
                return false;
            }

            DecoratorManager!.OnAdded(Owner, this, item, GetIndex(index));
            DecoratorManager!.OnRemoved(Owner, this, oldItem.Item, GetIndex(oldItem.OriginalIndex));
            return false;
        }

        private bool Remove(T item, int index)
        {
            var indexToRemove = IndexOf(index);
            _items.RemoveAt(indexToRemove);
            UpdateIndexes(index, -1);
            var limit = Limit!.Value;
            if (limit == 0 || indexToRemove >= limit)
                return false;

            if (_items.Count + 1 <= limit)
                return true;

            var oldItem = _items[limit - 1];
            if (index == oldItem.OriginalIndex)
            {
                DecoratorManager!.OnReplaced(Owner, this, item, oldItem.Item, GetIndex(index));
                return false;
            }

            DecoratorManager!.OnRemoved(Owner, this, item, GetIndex(index));
            DecoratorManager!.OnAdded(Owner, this, oldItem.Item, GetIndex(oldItem.OriginalIndex));

            return false;
        }

        private int GetIndex(int index)
        {
            if (Limit!.Value > index || Limit!.Value >= _items.Count)
                return index;

            var lastIndex = _items.BinarySearch(new ItemInfo(default!, index), this);
            if (lastIndex == 0)
                return index;
            if (lastIndex < 0)
                lastIndex = ~lastIndex;
            if (lastIndex <= Limit.Value)
                return index;
            return index - (lastIndex - Limit.Value);
        }

        private void UpdateIndexes(int index, int value)
        {
#if NET5_0
            var items = CollectionsMarshal.AsSpan(_items);
            for (var i = items.Length - 1; i >= 0; i--)
#else
            var items = _items;
            for (var i = items.Count - 1; i >= 0; i--)
#endif
            {
                var item = items[i];
                if (item.OriginalIndex >= index)
                {
#if NET5_0
                    items[i].OriginalIndex += value;
#else
                    items[i] = new ItemInfo(item.Item, item.OriginalIndex + value);
#endif
                }
                else
                    break;
            }
        }

        IEnumerable<object?> ICollectionDecorator.Decorate(ICollection collection, IEnumerable<object?> items) => HasCondition ? Decorate(items) : items;

        bool ICollectionDecorator.OnChanged(ICollection collection, ref object? item, ref int index, ref object? args)
        {
            if (!HasCondition)
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

            Replace(collection, oldSatisfied, newSatisfied, item, item, index);
            return false;
        }

        bool ICollectionDecorator.OnAdded(ICollection collection, ref object? item, ref int index)
        {
            if (!HasCondition)
                return true;

            if (IsSatisfied(item))
            {
                using var t = DecoratorManager!.BatchUpdate(collection, this);
                if (!Add((T) item!, index))
                    return false;
            }
            else
                UpdateIndexes(index, 1);

            index = GetIndex(index);
            return true;
        }

        bool ICollectionDecorator.OnReplaced(ICollection collection, ref object? oldItem, ref object? newItem, ref int index)
        {
            if (!HasCondition)
                return true;

            var oldSatisfied = IsSatisfied(oldItem);
            var newSatisfied = IsSatisfied(newItem);

            if (!oldSatisfied && !newSatisfied)
            {
                index = GetIndex(index);
                return true;
            }

            Replace(collection, oldSatisfied, newSatisfied, oldItem, newItem, index);
            return false;
        }

        bool ICollectionDecorator.OnMoved(ICollection collection, ref object? item, ref int oldIndex, ref int newIndex)
        {
            if (!HasCondition)
                return true;

            if (oldIndex < Limit && newIndex < Limit || !IsSatisfied(item))
            {
                var toRemove = IndexOf(oldIndex);
                UpdateIndexes(oldIndex + 1, -1);
                UpdateIndexes(newIndex, 1);
                if (toRemove != NotFound)
                {
                    _items.RemoveAt(toRemove);
                    MugenExtensions.AddOrdered(_items, new ItemInfo((T) item!, newIndex), this);
                }

                oldIndex = GetIndex(oldIndex);
                newIndex = GetIndex(newIndex);
                return true;
            }

            using var t = DecoratorManager!.BatchUpdate(collection, this);
            var value = (T) item!;
            Remove(value, oldIndex);
            Add(value, newIndex);
            return false;
        }

        bool ICollectionDecorator.OnRemoved(ICollection collection, ref object? item, ref int index)
        {
            if (!HasCondition)
                return true;

            if (IsSatisfied(item))
            {
                using var t = DecoratorManager!.BatchUpdate(collection, this);
                if (!Remove((T) item!, index))
                    return false;
            }
            else
                UpdateIndexes(index, -1);

            index = GetIndex(index);
            return true;
        }

        bool ICollectionDecorator.OnReset(ICollection collection, ref IEnumerable<object?>? items)
        {
            if (!HasCondition)
                return true;

            _items.Clear();
            if (items != null)
            {
                UpdateItems(items);
                items = Decorate(items);
            }

            return true;
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