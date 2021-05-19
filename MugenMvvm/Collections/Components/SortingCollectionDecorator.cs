using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using MugenMvvm.Components;
using MugenMvvm.Constants;
using MugenMvvm.Interfaces.Collections.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Collections.Components
{
    public class SortingCollectionDecorator : AttachableComponentBase<ICollection>, ICollectionDecorator, IReadOnlyCollection<object?>, IHasPriority
    {
        private readonly OrderedItemComparer _comparer;
        private readonly List<OrderedItem> _items;

        public SortingCollectionDecorator(IComparer<object?> comparer, int priority = CollectionComponentPriority.SortingDecorator)
        {
            Should.NotBeNull(comparer, nameof(comparer));
            _comparer = new OrderedItemComparer(comparer);
            _items = new List<OrderedItem>();
            Priority = priority;
        }

        public IComparer<object?> Comparer
        {
            get => _comparer.Comparer;
            set
            {
                Should.NotBeNull(value, nameof(value));
                ReorderInternal(value, true);
            }
        }

        public int Priority { get; set; }

        protected ICollectionDecoratorManagerComponent? DecoratorManager { get; private set; }

        int IReadOnlyCollection<object?>.Count => _items.Count;

        public void Reorder() => ReorderInternal(null, false);

        public IEnumerator<object?> GetEnumerator()
        {
            for (var i = 0; i < _items.Count; i++)
                yield return _items[i].Item;
        }

        protected override void OnAttached(ICollection owner, IReadOnlyMetadataContext? metadata) => DecoratorManager = CollectionDecoratorManager.GetOrAdd(owner);

        protected override void OnDetached(ICollection owner, IReadOnlyMetadataContext? metadata)
        {
            _items.Clear();
            DecoratorManager = null;
        }

        private void ReorderInternal(IComparer<object?>? comparer, bool setComparer)
        {
            if (DecoratorManager == null)
            {
                if (setComparer)
                    _comparer.Comparer = comparer!;
                return;
            }

            using var _ = DecoratorManager.TryLock(Owner, this);
            if (setComparer)
                _comparer.Comparer = comparer!;
            Reset(DecoratorManager.Decorate(Owner, this));
            DecoratorManager.OnReset(Owner, this, this);
        }

        private void Reset(IEnumerable<object?> items)
        {
            _items.Clear();
            var index = 0;
            foreach (var item in items)
                _items.Add(new OrderedItem(index++, item));
            _items.Sort(_comparer);
        }

        private int GetInsertIndex(object? item)
        {
            var num = _items.BinarySearch(new OrderedItem(-1, item), _comparer);
            if (num >= 0)
                return num;
            return ~num;
        }

        private int GetIndexByOriginalIndex(int index)
        {
#if NET5_0
            var items = CollectionsMarshal.AsSpan(_items);
            for (var i = 0; i < items.Length; i++)
#else
            var items = _items;
            for (var i = 0; i < items.Count; i++)
#endif
            {
                if (items[i].OriginalIndex == index)
                    return i;
            }

            return -1;
        }

        private void UpdateIndexes(int index, int value)
        {
            if (_items.Count == 0)
                return;

#if NET5_0
            var span = CollectionsMarshal.AsSpan(_items);
            for (var i = 0; i < span.Length; i++)
            {
                if (span[i].OriginalIndex >= index)
                    span[i].OriginalIndex += value;
            }
#else
            for (var i = 0; i < _items.Count; i++)
            {
                var item = _items[i];
                if (item.OriginalIndex < index)
                    continue;

                item.UpdateIndex(value);
                _items[i] = item;
            }
#endif
        }

        IEnumerable<object?> ICollectionDecorator.Decorate(ICollection collection, IEnumerable<object?> items) =>
            DecoratorManager == null ? items : this;

        bool ICollectionDecorator.OnChanged(ICollection collection, ref object? item, ref int index, ref object? args)
        {
            if (DecoratorManager == null)
                return false;

            var oldIndex = GetIndexByOriginalIndex(index);
            if (oldIndex == -1)
                return false;

            _items.RemoveAt(oldIndex);
            var newIndex = GetInsertIndex(item);
            _items.Insert(newIndex, new OrderedItem(index, item));
            if (oldIndex == newIndex)
                index = oldIndex;
            else
            {
                DecoratorManager.OnMoved(collection, this, item, oldIndex, newIndex);
                index = newIndex;
            }

            return true;
        }

        bool ICollectionDecorator.OnAdded(ICollection collection, ref object? item, ref int index)
        {
            if (DecoratorManager == null)
                return false;

            UpdateIndexes(index, 1);
            var newIndex = GetInsertIndex(item);
            _items.Insert(newIndex, new OrderedItem(index, item));
            index = newIndex;
            return true;
        }

        bool ICollectionDecorator.OnReplaced(ICollection collection, ref object? oldItem, ref object? newItem, ref int index)
        {
            if (DecoratorManager == null)
                return false;

            var oldIndex = GetIndexByOriginalIndex(index);
            if (oldIndex == -1)
                return false;

            using var _ = DecoratorManager.BatchUpdate(collection, this);
            _items.RemoveAt(oldIndex);
            DecoratorManager.OnRemoved(collection, this, oldItem, oldIndex);

            var newIndex = GetInsertIndex(newItem);
            _items.Insert(newIndex, new OrderedItem(index, newItem));
            DecoratorManager.OnAdded(collection, this, newItem, newIndex);
            return false;
        }

        bool ICollectionDecorator.OnMoved(ICollection collection, ref object? item, ref int oldIndex, ref int newIndex)
        {
            if (DecoratorManager == null)
                return false;

            var index = GetIndexByOriginalIndex(oldIndex);
            UpdateIndexes(oldIndex + 1, -1);
            UpdateIndexes(newIndex, 1);

            if (index != -1)
            {
                var orderedItem = _items[index];
                orderedItem.OriginalIndex = newIndex;
                _items[index] = orderedItem;
            }

            return false;
        }

        bool ICollectionDecorator.OnRemoved(ICollection collection, ref object? item, ref int index)
        {
            if (DecoratorManager == null)
                return false;

            var indexToRemove = GetIndexByOriginalIndex(index);
            UpdateIndexes(index, -1);
            if (indexToRemove == -1)
                return false;

            _items.RemoveAt(indexToRemove);
            index = indexToRemove;
            return true;
        }

        bool ICollectionDecorator.OnReset(ICollection collection, ref IEnumerable<object?>? items)
        {
            if (DecoratorManager == null)
                return false;

            if (items == null)
                _items.Clear();
            else
            {
                Reset(items);
                items = this;
            }

            return true;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        [StructLayout(LayoutKind.Auto)]
        private struct OrderedItem
        {
            public readonly object? Item;
            public int OriginalIndex;

            public OrderedItem(int originalIndex, object? item)
            {
                OriginalIndex = originalIndex;
                Item = item;
            }

#if !NET5_0
            public void UpdateIndex(int index) => OriginalIndex += index;
#endif
        }

        private sealed class OrderedItemComparer : IComparer<OrderedItem>
        {
            public IComparer<object?> Comparer;

            public OrderedItemComparer(IComparer<object?> comparer)
            {
                Comparer = comparer;
            }

            int IComparer<OrderedItem>.Compare(OrderedItem x, OrderedItem y) => Comparer.Compare(x.Item, y.Item);
        }
    }
}