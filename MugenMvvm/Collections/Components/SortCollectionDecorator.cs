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
    public class SortCollectionDecorator : CollectionDecoratorBase, IReadOnlyCollection<object?>, IComparer<SortCollectionDecorator.OrderedItem>
    {
        private ListInternal<OrderedItem> _items;
        private IComparer<object?>? _comparer;

        public SortCollectionDecorator(IComparer<object?>? comparer = null, int priority = CollectionComponentPriority.SortingDecorator) : base(priority)
        {
            _comparer = comparer;
            _items = new ListInternal<OrderedItem>(8);
            Priority = priority;
        }

        public override bool HasAdditionalItems => false;

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
            if (Comparer == null)
                return true;

            var oldIndex = GetIndexByOriginalIndex(item, index);
            _items.RemoveAt(oldIndex);
            var newIndex = _items.AddOrdered(new OrderedItem(index, item), this);
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
            index = _items.AddOrdered(new OrderedItem(index, item), this);
            return true;
        }

        protected override bool OnReplaced(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref object? oldItem,
            ref object? newItem, ref int index)
        {
            if (Comparer == null)
                return true;

            var oldIndex = GetIndexByOriginalIndex(oldItem, index);
            if (oldIndex == -1)
                return false;

            _items.RemoveAt(oldIndex);
            decoratorManager.OnRemoved(collection, this, oldItem, oldIndex);
            decoratorManager.OnAdded(collection, this, newItem, _items.AddOrdered(new OrderedItem(index, newItem), this));
            return false;
        }

        protected override bool OnMoved(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref object? item, ref int oldIndex,
            ref int newIndex)
        {
            if (Comparer == null)
                return true;

            var index = GetIndexByOriginalIndex(item, oldIndex);
            UpdateIndexes(oldIndex + 1, -1);
            UpdateIndexes(newIndex, 1);

            if (index != -1)
                _items.Items[index].OriginalIndex = newIndex;

            return false;
        }

        protected override bool OnRemoved(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref object? item, ref int index)
        {
            if (Comparer == null)
                return true;

            var indexToRemove = GetIndexByOriginalIndex(item, index);
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
            if (items is IReadOnlyCollection<object?> c)
                _items.EnsureCapacity(c.Count);

            _items.Clear();
            var index = 0;
            foreach (var item in items)
                _items.Add(new OrderedItem(index++, item));
            _items.Sort(this);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetIndexByOriginalIndex(object? item, int index)
        {
            var binarySearchIndex = _items.BinarySearch(new OrderedItem(index, item), this);
            if (binarySearchIndex >= 0)
                return binarySearchIndex;
            return GetIndexByOriginalIndexFallback(index);
        }

        private int GetIndexByOriginalIndexFallback(int index)
        {
            var count = _items.Count;
            var items = _items.Items;
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
            if (index == 0)
            {
                for (var i = 0; i < count; i++)
                    items[i].OriginalIndex += value;
            }
            else
            {
                var countToUpdate = count - index;
                if (countToUpdate == 0)
                    return;

                for (var i = 0; i < count; i++)
                {
                    if (items[i].OriginalIndex >= index)
                    {
                        items[i].OriginalIndex += value;
                        if (--countToUpdate == 0)
                            break;
                    }
                }
            }
        }

        int IComparer<OrderedItem>.Compare(OrderedItem x, OrderedItem y)
        {
            if (Comparer == null)
                return 0;
            var result = Comparer.Compare(x.Item, y.Item);
            if (result == 0)
                return x.OriginalIndex.CompareTo(y.OriginalIndex);
            return result;
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