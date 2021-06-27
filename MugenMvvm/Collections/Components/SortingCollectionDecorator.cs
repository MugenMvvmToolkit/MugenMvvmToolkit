using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MugenMvvm.Constants;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Collections.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Collections.Components
{
    public class SortingCollectionDecorator : CollectionDecoratorBase, IReadOnlyCollection<object?>, IComparer<SortingCollectionDecorator.OrderedItem>
    {
        private readonly ListInternal<OrderedItem> _items;
        private IComparer<object?>? _comparer;

        public SortingCollectionDecorator(IComparer<object?>? comparer = null, int priority = CollectionComponentPriority.SortingDecorator) : base(priority)
        {
            _comparer = comparer;
            _items = new ListInternal<OrderedItem>(8);
            Priority = priority;
        }

        public IComparer<object?>? Comparer
        {
            get => _comparer;
            set => ReorderInternal(value, true);
        }

        int IReadOnlyCollection<object?>.Count => _items.Count;

        public void Reorder() => ReorderInternal(null, false);

        public IEnumerator<object?> GetEnumerator()
        {
            var count = _items.Count;
            var items = _items.Items;
            for (var i = 0; i < count; i++)
                yield return items[i].Item;
        }

        protected override void OnDetached(IReadOnlyObservableCollection owner, IReadOnlyMetadataContext? metadata)
        {
            base.OnDetached(owner, metadata);
            _items.Clear();
        }

        protected override IEnumerable<object?> Decorate(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection,
            IEnumerable<object?> items) => Comparer == null ? items : this;

        protected override bool OnChanged(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref object? item, ref int index,
            ref object? args)
        {
            var comparer = Comparer;
            if (comparer == null)
                return true;

            var oldIndex = GetIndexByOriginalIndex(comparer, item, index);
            if (oldIndex == -1)
                return false;

            _items.RemoveAt(oldIndex);
            var newIndex = GetInsertIndex(item);
            _items.Insert(newIndex, new OrderedItem(index, item));
            if (oldIndex == newIndex)
                index = oldIndex;
            else
            {
                decoratorManager.OnMoved(collection, this, item, oldIndex, newIndex);
                index = newIndex;
            }

            return true;
        }

        protected override bool OnAdded(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref object? item, ref int index)
        {
            if (Comparer == null)
                return true;

            UpdateIndexes(index, 1);
            var newIndex = GetInsertIndex(item);
            _items.Insert(newIndex, new OrderedItem(index, item));
            index = newIndex;
            return true;
        }

        protected override bool OnReplaced(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref object? oldItem,
            ref object? newItem, ref int index)
        {
            var comparer = Comparer;
            if (comparer == null)
                return true;

            var oldIndex = GetIndexByOriginalIndex(comparer, oldItem, index);
            if (oldIndex == -1)
                return false;

            _items.RemoveAt(oldIndex);
            decoratorManager.OnRemoved(collection, this, oldItem, oldIndex);

            var newIndex = GetInsertIndex(newItem);
            _items.Insert(newIndex, new OrderedItem(index, newItem));
            decoratorManager.OnAdded(collection, this, newItem, newIndex);
            return false;
        }

        protected override bool OnMoved(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref object? item, ref int oldIndex,
            ref int newIndex)
        {
            var comparer = Comparer;
            if (comparer == null)
                return true;

            var index = GetIndexByOriginalIndex(comparer, item, oldIndex);
            UpdateIndexes(oldIndex + 1, -1);
            UpdateIndexes(newIndex, 1);

            if (index != -1)
                _items.Items[index].OriginalIndex = newIndex;

            return false;
        }

        protected override bool OnRemoved(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref object? item, ref int index)
        {
            var comparer = Comparer;
            if (comparer == null)
                return true;

            var indexToRemove = GetIndexByOriginalIndex(comparer, item, index);
            UpdateIndexes(index, -1);
            if (indexToRemove == -1)
                return false;

            _items.RemoveAt(indexToRemove);
            index = indexToRemove;
            return true;
        }

        protected override bool OnReset(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref IEnumerable<object?>? items)
        {
            if (Comparer == null)
                return true;

            if (items == null)
                _items.Clear();
            else
            {
                Reset(items);
                items = this;
            }

            return true;
        }

        private void ReorderInternal(IComparer<object?>? comparer, bool setComparer)
        {
            if (DecoratorManager == null)
            {
                if (setComparer)
                    _comparer = comparer!;
                return;
            }

            using var _ = Owner.TryLock();
            if (setComparer)
                _comparer = comparer!;
            if (Comparer == null)
            {
                _items.Clear();
                DecoratorManager.OnReset(Owner, this, DecoratorManager.Decorate(Owner, this));
            }
            else
            {
                Reset(DecoratorManager.Decorate(Owner, this));
                DecoratorManager.OnReset(Owner, this, this);
            }
        }

        private void Reset(IEnumerable<object?> items)
        {
            _items.Clear();
            var index = 0;
            foreach (var item in items)
                _items.Add(new OrderedItem(index++, item));
            _items.Sort(this);
        }

        private int GetInsertIndex(object? item)
        {
            var num = _items.BinarySearch(new OrderedItem(-1, item), this);
            if (num >= 0)
                return num;
            return ~num;
        }

        private int GetIndexByOriginalIndex(IComparer<object?> comparer, object? item, int index)
        {
            var count = _items.Count;
            var items = _items.Items;
            var startIndex = _items.BinarySearch(new OrderedItem(-1, item), this);
            if (startIndex >= 0)
            {
                var value = items[startIndex];
                if (value.OriginalIndex == index)
                    return startIndex;

                var leftIndex = startIndex - 1;
                var rightIndex = startIndex + 1;
                do
                {
                    if (rightIndex < count)
                    {
                        value = items[rightIndex];
                        if (comparer.Compare(item, value.Item) != 0)
                            rightIndex = int.MaxValue;
                        else if (value.OriginalIndex == index)
                            return rightIndex;
                        else
                            ++rightIndex;
                    }

                    if (leftIndex >= 0)
                    {
                        value = items[leftIndex];
                        if (comparer.Compare(item, value.Item) != 0)
                            leftIndex = int.MinValue;
                        else if (value.OriginalIndex == index)
                            return leftIndex;
                        else
                            --leftIndex;
                    }
                } while (rightIndex < count || leftIndex >= 0);
            }

            //fallback
            for (var i = 0; i < count; i++)
            {
                if (items[i].OriginalIndex == index)
                    return i;
            }

            return -1;
        }

        private void UpdateIndexes(int index, int value)
        {
            var count = _items.Count;
            var items = _items.Items;
            for (var i = 0; i < count; i++)
            {
                if (items[i].OriginalIndex >= index)
                    items[i].OriginalIndex += value;
            }
        }

        int IComparer<OrderedItem>.Compare(OrderedItem x, OrderedItem y)
        {
            if (Comparer == null)
                return 0;
            return Comparer.Compare(x.Item, y.Item);
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        [StructLayout(LayoutKind.Auto)]
        private struct OrderedItem
        {
            public readonly object? Item;
            public int OriginalIndex;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public OrderedItem(int originalIndex, object? item)
            {
                OriginalIndex = originalIndex;
                Item = item;
            }
        }
    }
}