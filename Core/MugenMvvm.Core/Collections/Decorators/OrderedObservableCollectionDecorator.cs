using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Collections.Decorators
{
    public sealed class OrderedObservableCollectionDecorator<T> : IObservableCollectionDecorator<T>
    {
        #region Fields

        private readonly OrderedItemComparer _comparer;

        private readonly List<OrderedItem> _items;
        private IObservableCollectionDecoratorManager<T>? _decoratorManager;

        #endregion

        #region Constructors

        public OrderedObservableCollectionDecorator(IComparer<T> comparer)
        {
            Should.NotBeNull(comparer, nameof(comparer));
            _comparer = new OrderedItemComparer(comparer);
            _items = new List<OrderedItem>();
        }

        #endregion

        #region Properties

        public IComparer<T> Comparer => _comparer.Comparer;

        private IEnumerable<T> Items => _items.Select(item => item.Item);

        #endregion

        #region Implementation of interfaces

        void IAttachableComponent<IObservableCollectionDecoratorManager<T>>.OnAttached(IObservableCollectionDecoratorManager<T> owner)
        {
            _decoratorManager = owner;
            using (owner.Lock())
            {
                Reset(owner.DecorateItems(this));
            }
        }

        IEnumerable<T> IObservableCollectionDecorator<T>.DecorateItems(IEnumerable<T> items)
        {
            return items.OrderBy(arg => arg, Comparer);
        }

        bool IObservableCollectionDecorator<T>.OnItemChanged(ref T item, ref int index, ref object? args)
        {
            if (_decoratorManager == null)
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
                _decoratorManager.OnMoved(this, item, oldIndex, newIndex);
                index = newIndex;
            }

            return true;
        }

        bool IObservableCollectionDecorator<T>.OnAdded(ref T item, ref int index)
        {
            UpdateIndexes(index, 1);
            var newIndex = GetInsertIndex(item);
            _items.Insert(newIndex, new OrderedItem(index, item));
            index = newIndex;
            return true;
        }

        bool IObservableCollectionDecorator<T>.OnReplaced(ref T oldItem, ref T newItem, ref int index)
        {
            if (_decoratorManager == null)
                return false;

            var oldIndex = GetIndexByOriginalIndex(index);
            if (oldIndex == -1)
                return false;

            _items.RemoveAt(oldIndex);
            _decoratorManager.OnRemoved(this, oldItem, oldIndex);

            var newIndex = GetInsertIndex(newItem);
            _items.Insert(newIndex, new OrderedItem(index, newItem));
            _decoratorManager.OnAdded(this, newItem, newIndex);
            return false;
        }

        bool IObservableCollectionDecorator<T>.OnMoved(ref T item, ref int oldIndex, ref int newIndex)
        {
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

        bool IObservableCollectionDecorator<T>.OnRemoved(ref T item, ref int index)
        {
            var indexToRemove = GetIndexByOriginalIndex(index);
            UpdateIndexes(index, -1);
            if (indexToRemove == -1)
                return false;

            _items.RemoveAt(indexToRemove);
            index = indexToRemove;
            return true;
        }

        public bool OnReset(ref IEnumerable<T> items)
        {
            Reset(items);
            items = Items;
            return true;
        }

        public bool OnCleared()
        {
            _items.Clear();
            return true;
        }

        #endregion

        #region Methods

        public void Reorder()
        {
            if (_decoratorManager == null)
                return;

            using (_decoratorManager.Lock())
            {
                Reset(_decoratorManager.DecorateItems(this));
                _decoratorManager.OnReset(this, Items);
            }
        }

        private void Reset(IEnumerable<T> items)
        {
            _items.Clear();
            _items.AddRange(items.Select((arg1, i) => new OrderedItem(i, arg1)));
            _items.Sort(_comparer);
        }

        private int GetInsertIndex(T item)
        {
            Should.NotBeNull(item, nameof(item));
            var num = _items.BinarySearch(new OrderedItem(-1, item), _comparer);
            if (num >= 0)
                return num;
            return ~num;
        }

        private int GetIndexByOriginalIndex(int index)
        {
            for (var i = 0; i < _items.Count; i++)
            {
                if (_items[i].OriginalIndex == index)
                    return i;
            }

            return -1;
        }

        private void UpdateIndexes(int index, int value)
        {
            if (_items.Count == 0)
                return;

            for (var i = 0; i < _items.Count; i++)
            {
                var item = _items[i];
                if (item.OriginalIndex < index)
                    continue;
                item.UpdateIndex(value);
                _items[i] = item;
            }
        }

        #endregion

        #region Nested types

        private sealed class OrderedItemComparer : IComparer<OrderedItem>
        {
            #region Fields

            public readonly IComparer<T> Comparer;

            #endregion

            #region Constructors

            public OrderedItemComparer(IComparer<T> comparer)
            {
                Comparer = comparer;
            }

            #endregion

            #region Implementation of interfaces

            int IComparer<OrderedItem>.Compare(OrderedItem x, OrderedItem y)
            {
                return Comparer.Compare(x.Item, y.Item);
            }

            #endregion
        }

        private struct OrderedItem
        {
            #region Fields

            public readonly T Item;
            public int OriginalIndex;

            #endregion

            #region Constructors

            public OrderedItem(int originalIndex, T item)
            {
                OriginalIndex = originalIndex;
                Item = item;
            }

            #endregion

            #region Methods

            public void UpdateIndex(int index)
            {
                OriginalIndex += index;
            }

            #endregion
        }

        #endregion
    }
}