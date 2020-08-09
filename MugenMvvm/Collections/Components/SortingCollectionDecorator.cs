using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using MugenMvvm.Components;
using MugenMvvm.Constants;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Collections.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Collections.Components
{
    public sealed class SortingCollectionDecorator : AttachableComponentBase<IObservableCollection>, ICollectionDecorator, IEnumerable<object?>, IHasPriority
    {
        #region Fields

        private readonly OrderedItemComparer _comparer;
        private readonly List<OrderedItem> _items;
        private ICollectionDecoratorManagerComponent? _decoratorManager;

        #endregion

        #region Constructors

        public SortingCollectionDecorator(IComparer<object?> comparer)
        {
            Should.NotBeNull(comparer, nameof(comparer));
            _comparer = new OrderedItemComparer(comparer);
            _items = new List<OrderedItem>();
        }

        #endregion

        #region Properties

        public IComparer<object?> Comparer => _comparer.Comparer;

        public int Priority { get; set; } = CollectionComponentPriority.OrderDecorator;

        #endregion

        #region Implementation of interfaces

        IEnumerable<object?> ICollectionDecorator.DecorateItems(IObservableCollection observableCollection, IEnumerable<object?> items) => items.OrderBy(arg => arg, Comparer);

        bool ICollectionDecorator.OnItemChanged(IObservableCollection observableCollection, ref object? item, ref int index, ref object? args)
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
                _decoratorManager.OnMoved(observableCollection, this, item, oldIndex, newIndex);
                index = newIndex;
            }

            return true;
        }

        bool ICollectionDecorator.OnAdded(IObservableCollection observableCollection, ref object? item, ref int index)
        {
            UpdateIndexes(index, 1);
            var newIndex = GetInsertIndex(item);
            _items.Insert(newIndex, new OrderedItem(index, item));
            index = newIndex;
            return true;
        }

        bool ICollectionDecorator.OnReplaced(IObservableCollection observableCollection, ref object? oldItem, ref object? newItem, ref int index)
        {
            if (_decoratorManager == null)
                return false;

            var oldIndex = GetIndexByOriginalIndex(index);
            if (oldIndex == -1)
                return false;

            _items.RemoveAt(oldIndex);
            _decoratorManager.OnRemoved(observableCollection, this, oldItem, oldIndex);

            var newIndex = GetInsertIndex(newItem);
            _items.Insert(newIndex, new OrderedItem(index, newItem));
            _decoratorManager.OnAdded(observableCollection, this, newItem, newIndex);
            return false;
        }

        bool ICollectionDecorator.OnMoved(IObservableCollection observableCollection, ref object? item, ref int oldIndex, ref int newIndex)
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

        bool ICollectionDecorator.OnRemoved(IObservableCollection observableCollection, ref object? item, ref int index)
        {
            var indexToRemove = GetIndexByOriginalIndex(index);
            UpdateIndexes(index, -1);
            if (indexToRemove == -1)
                return false;

            _items.RemoveAt(indexToRemove);
            index = indexToRemove;
            return true;
        }

        public bool OnReset(IObservableCollection observableCollection, ref IEnumerable<object?> items)
        {
            Reset(items);
            items = this;
            return true;
        }

        public bool OnCleared(IObservableCollection observableCollection)
        {
            _items.Clear();
            return true;
        }

        public IEnumerator<object?> GetEnumerator()
        {
            for (var i = 0; i < _items.Count; i++)
                yield return _items[i].Item;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion

        #region Methods

        protected override void OnAttached(IObservableCollection owner, IReadOnlyMetadataContext? metadata)
        {
            _decoratorManager = owner.GetOrAddCollectionDecoratorManager();
            Reorder();
        }

        protected override void OnDetached(IObservableCollection owner, IReadOnlyMetadataContext? metadata)
        {
            _items.Clear();
            _decoratorManager = null;
        }

        public void Reorder()
        {
            if (_decoratorManager == null)
                return;

            using (Owner.TryLock())
            {
                Reset(_decoratorManager.DecorateItems(Owner, this));
                _decoratorManager.OnReset(Owner, this, this);
            }
        }

        private void Reset(IEnumerable<object?> items)
        {
            _items.Clear();
            _items.AddRange(items.Select((arg1, i) => new OrderedItem(i, arg1)));
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

            public readonly IComparer<object?> Comparer;

            #endregion

            #region Constructors

            public OrderedItemComparer(IComparer<object?> comparer)
            {
                Comparer = comparer;
            }

            #endregion

            #region Implementation of interfaces

            int IComparer<OrderedItem>.Compare(OrderedItem x, OrderedItem y) => Comparer.Compare(x.Item, y.Item);

            #endregion
        }

        [StructLayout(LayoutKind.Auto)]
        private struct OrderedItem
        {
            #region Fields

            public readonly object? Item;
            public int OriginalIndex;

            #endregion

            #region Constructors

            public OrderedItem(int originalIndex, object? item)
            {
                OriginalIndex = originalIndex;
                Item = item;
            }

            #endregion

            #region Methods

            public void UpdateIndex(int index) => OriginalIndex += index;

            #endregion
        }

        #endregion
    }
}