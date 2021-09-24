using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Collections.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Collections.Components
{
    public sealed class SortCollectionDecorator<TState> : CollectionDecoratorBase, IReadOnlyList<object?>, IComparer<SortCollectionDecorator<TState>.OrderedItem>, IHasCache
    {
        private readonly Func<object?, TState?> _getState;
        private ListInternal<OrderedItem> _items;
        private IComparer<TState?>? _comparer;

        public SortCollectionDecorator(int priority, Func<object?, TState?> getState, IComparer<TState?>? comparer = null) : base(priority)
        {
            Should.NotBeNull(getState, nameof(getState));
            _getState = getState;
            _comparer = comparer;
            _items = new ListInternal<OrderedItem>(8);
            Priority = priority;
        }

        public IComparer<TState?>? Comparer
        {
            get => _comparer;
            set => ReorderInternal(value, true);
        }

        protected override bool HasAdditionalItems => false;

        int IReadOnlyCollection<object?>.Count => _items.Count;

        object? IReadOnlyList<object?>.this[int index] => _items.Items[index].Item;

        public IEnumerator<object?> GetEnumerator()
        {
            var count = _items.Count;
            var items = _items.Items;
            for (var i = 0; i < count; i++)
                yield return items[i].Item;
        }

        public void Invalidate(object? state = null, IReadOnlyMetadataContext? metadata = null) => ReorderInternal(null, false);

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

            var state = _getState(item);
            if (IsChanged(item, index, state, out var oldIndex))
            {
                _items.RemoveAt(oldIndex);
                var newIndex = _items.AddOrdered(new OrderedItem(index, item, state), this);
                if (oldIndex == newIndex)
                    index = oldIndex;
                else
                {
                    decoratorManager.OnMoved(collection, this, item, oldIndex, newIndex);
                    index = newIndex;
                }
            }
            else
                index = oldIndex;

            return true;
        }

        protected override bool OnAdded(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref object? item, ref int index)
        {
            if (Comparer == null)
                return true;

            UpdateIndexes(index, 1);
            index = _items.AddOrdered(new OrderedItem(index, item, _getState(item)), this);
            return true;
        }

        protected override bool OnReplaced(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref object? oldItem,
            ref object? newItem, ref int index)
        {
            if (Comparer == null)
                return true;

            var oldIndex = GetIndexByOriginalIndex(oldItem, index, _getState(oldItem));
            var oldState = _items.Items[oldIndex].State;
            var newState = _getState(newItem);
            if (Comparer.Compare(oldState, newState) != 0)
            {
                _items.RemoveAt(oldIndex);
                decoratorManager.OnRemoved(collection, this, oldItem, oldIndex);
                decoratorManager.OnAdded(collection, this, newItem, _items.AddOrdered(new OrderedItem(index, newItem, newState), this));
                return false;
            }

            _items.Items[oldIndex] = new OrderedItem(index, newItem, newState);
            return true;
        }

        protected override bool OnMoved(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref object? item, ref int oldIndex,
            ref int newIndex)
        {
            if (Comparer == null)
                return true;

            var index = GetIndexByOriginalIndex(item, oldIndex, _getState(item));
            UpdateIndexesMove(oldIndex, newIndex);

            if (index != -1)
                _items.Items[index].OriginalIndex = newIndex;

            return false;
        }

        protected override bool OnRemoved(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref object? item, ref int index)
        {
            if (Comparer == null)
                return true;

            var indexToRemove = GetIndexByOriginalIndex(item, index, _getState(item));
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

            _items.Clear();
            if (items != null)
            {
                Reset(items);
                items = this;
            }

            return true;
        }

        private bool IsChanged(object? item, int originalIndex, TState? state, out int oldIndex)
        {
            var currentItem = new OrderedItem(originalIndex, item, state);
            oldIndex = _items.BinarySearch(currentItem, this);
            if (oldIndex < 0)
            {
                oldIndex = GetIndexByOriginalIndexFallback(originalIndex);
                return true;
            }

            if (oldIndex != 0 && Compare(_items.Items[oldIndex - 1], currentItem) > 0)
                return true;
            if (oldIndex < _items.Count - 1 && Compare(_items.Items[oldIndex + 1], currentItem) < 0)
                return true;
            return false;
        }

        private void ReorderInternal(IComparer<TState>? comparer, bool setComparer)
        {
            var decoratorManager = DecoratorManager;
            var owner = OwnerOptional;
            if (decoratorManager == null || owner == null)
            {
                if (setComparer)
                    _comparer = comparer!;
                return;
            }

            using var _ = owner.Lock();
            if (setComparer)
                _comparer = comparer!;
            if (DecoratorManager == null)
                return;

            if (Comparer == null)
            {
                _items.Clear();
                decoratorManager.OnReset(owner, this);
            }
            else
            {
                _items.Clear();
                Reset(decoratorManager.Decorate(owner, this));
                decoratorManager.OnReset(owner, this, this);
            }
        }

        private void Reset(IEnumerable<object?> items)
        {
            if (items.TryGetCount(out var count))
            {
                if (count == 0)
                    return;
                _items.EnsureCapacity(count);
            }

            var index = 0;
            foreach (var item in items)
                _items.Add(new OrderedItem(index++, item, _getState(item)));
            _items.Sort(this);
        }

        private int GetIndexByOriginalIndex(object? item, int index, TState? state)
        {
            var binarySearchIndex = _items.BinarySearch(new OrderedItem(index, item, state), this);
            if (binarySearchIndex < 0)
                return GetIndexByOriginalIndexFallback(index);
            return binarySearchIndex;
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

        private void UpdateIndexesMove(int oldIndex, int newIndex)
        {
            var count = _items.Count;
            var items = _items.Items;
            if (oldIndex < newIndex)
            {
                var countToUpdate = newIndex - oldIndex;
                for (var i = 0; i < count; i++)
                {
                    var originalIndex = items[i].OriginalIndex;
                    if (originalIndex > oldIndex && originalIndex <= newIndex)
                    {
                        items[i].OriginalIndex = originalIndex - 1;
                        if (--countToUpdate == 0)
                            break;
                    }
                }
            }
            else
            {
                var countToUpdate = oldIndex - newIndex;
                for (var i = 0; i < count; i++)
                {
                    var originalIndex = items[i].OriginalIndex;
                    if (originalIndex >= newIndex && originalIndex < oldIndex)
                    {
                        items[i].OriginalIndex = originalIndex + 1;
                        if (--countToUpdate == 0)
                            break;
                    }
                }
            }
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

        private int Compare(OrderedItem x, OrderedItem y)
        {
            if (Comparer == null)
                return 0;
            var result = Comparer.Compare(x.State, y.State);
            if (result == 0)
                return x.OriginalIndex.CompareTo(y.OriginalIndex);
            return result;
        }

        int IComparer<OrderedItem>.Compare(OrderedItem x, OrderedItem y) => Compare(x, y);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        [StructLayout(LayoutKind.Auto)]
        private struct OrderedItem
        {
            public readonly object? Item;
            public readonly TState? State;
            public int OriginalIndex;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public OrderedItem(int originalIndex, object? item, TState? state)
            {
                OriginalIndex = originalIndex;
                Item = item;
                State = state;
            }
        }
    }
}