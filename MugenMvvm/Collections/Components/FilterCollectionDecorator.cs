using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Components;
using MugenMvvm.Constants;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Collections.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;

namespace MugenMvvm.Collections.Components
{
    public sealed class FilterCollectionDecorator<T> : AttachableComponentBase<IObservableCollection>, ICollectionDecorator, IEnumerable<object?>, IHasPriority
    {
        #region Fields

        private readonly Func<object?, bool> _internalFilter;
        private ICollectionDecoratorManagerComponent? _decoratorManager;
        private Func<T, bool>? _filter;

        private int[] _keys;
        private int _size;
        private object?[] _values;

        #endregion

        #region Constructors

        public FilterCollectionDecorator()
        {
            _keys = Default.Array<int>();
            _values = Default.Array<object?>();
            _size = 0;
            _internalFilter = FilterInternal;
        }

        #endregion

        #region Properties

        public Func<T, bool>? Filter
        {
            get => _filter;
            set
            {
                if (_filter != value)
                    UpdateFilterInternal(value, true);
            }
        }

        public int Priority { get; set; } = CollectionComponentPriority.FilterDecorator;

        private bool HasFilter => _filter != null && _decoratorManager != null;

        #endregion

        #region Implementation of interfaces

        IEnumerable<object?> ICollectionDecorator.DecorateItems(IObservableCollection observableCollection, IEnumerable<object?> items) => HasFilter ? items.Where(_internalFilter) : items;

        bool ICollectionDecorator.OnItemChanged(IObservableCollection observableCollection, ref object? item, ref int index, ref object? args)
        {
            if (!HasFilter)
                return true;


            var filterIndex = IndexOfKey(index);
            if (FilterInternal(item))
            {
                if (filterIndex == -1)
                {
                    index = Add(index, item);
                    _decoratorManager!.OnAdded(observableCollection, this, item, index);
                }
                else
                    index = filterIndex;

                return true;
            }

            if (filterIndex != -1)
            {
                RemoveAt(filterIndex);
                _decoratorManager!.OnRemoved(observableCollection, this, item, filterIndex);
            }

            return false;
        }

        bool ICollectionDecorator.OnAdded(IObservableCollection observableCollection, ref object? item, ref int index)
        {
            if (!HasFilter)
                return true;

            UpdateIndexes(index, 1);
            if (!FilterInternal(item))
                return false;

            index = Add(index, item);
            return true;
        }

        bool ICollectionDecorator.OnReplaced(IObservableCollection observableCollection, ref object? oldItem, ref object? newItem, ref int index)
        {
            if (!HasFilter)
                return true;

            var filterIndex = IndexOfKey(index);
            if (filterIndex == -1)
            {
                if (FilterInternal(newItem))
                    _decoratorManager!.OnAdded(observableCollection, this, newItem, Add(index, newItem));

                return false;
            }

            if (FilterInternal(newItem))
            {
                oldItem = GetValue(filterIndex)!;
                SetValue(filterIndex, newItem);
                index = filterIndex;
                return true;
            }

            var oldValue = GetValue(filterIndex);
            RemoveAt(filterIndex);
            _decoratorManager!.OnRemoved(observableCollection, this, oldValue, filterIndex);
            return false;
        }

        bool ICollectionDecorator.OnMoved(IObservableCollection observableCollection, ref object? item, ref int oldIndex, ref int newIndex)
        {
            if (!HasFilter)
                return true;

            var filterIndex = IndexOfKey(oldIndex);
            UpdateIndexes(oldIndex + 1, -1);
            UpdateIndexes(newIndex, 1);

            if (filterIndex == -1)
                return false;

            RemoveAt(filterIndex);
            oldIndex = filterIndex;
            newIndex = Add(newIndex, item);
            return true;
        }

        bool ICollectionDecorator.OnRemoved(IObservableCollection observableCollection, ref object? item, ref int index)
        {
            if (!HasFilter)
                return true;

            var filterIndex = IndexOfKey(index);
            UpdateIndexes(index, -1);
            if (filterIndex == -1)
                return false;

            RemoveAt(filterIndex);
            index = filterIndex;
            return true;
        }

        bool ICollectionDecorator.OnReset(IObservableCollection observableCollection, ref IEnumerable<object?> items)
        {
            if (!HasFilter)
                return true;

            Clear();
            UpdateItems(items);
            items = this;
            return true;
        }

        bool ICollectionDecorator.OnCleared(IObservableCollection observableCollection)
        {
            Clear();
            return true;
        }

        public IEnumerator<object?> GetEnumerator()
        {
            for (var i = 0; i < _size; i++)
                yield return _values[i];
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion

        #region Methods

        protected override void OnAttached(IObservableCollection owner, IReadOnlyMetadataContext? metadata)
        {
            _decoratorManager = owner.GetOrAddCollectionDecoratorManager();
            UpdateFilter();
        }

        protected override void OnDetached(IObservableCollection owner, IReadOnlyMetadataContext? metadata)
        {
            Clear();
            _decoratorManager = null;
        }

        public void UpdateFilter() => UpdateFilterInternal(null, false);

        private void UpdateFilterInternal(Func<T, bool>? filter, bool setFilter)
        {
            if (_decoratorManager == null)
            {
                if (setFilter)
                    _filter = filter;
                return;
            }

            using (Owner.TryLock())
            {
                if (setFilter)
                    _filter = filter;
                Clear();
                if (HasFilter)
                    UpdateItems(_decoratorManager.DecorateItems(Owner, this));
                _decoratorManager.OnReset(Owner, this, this);
            }
        }

        private void UpdateItems(IEnumerable<object?> items)
        {
            var index = 0;
            foreach (var item in items)
            {
                if (FilterInternal(item))
                    Add(index, item);
                ++index;
            }
        }

        private bool FilterInternal(object? value) => _filter == null || !(value is T v) || _filter(v);

        private void UpdateIndexes(int index, int value)
        {
            if (_size == 0)
                return;

            var start = IndexOfKey(index);
            if (start == -1)
            {
                if (_keys[_size - 1] < index)
                    return;
                for (var i = 0; i < _size; i++)
                {
                    var key = _keys[i];
                    if (key < index)
                        continue;
                    _keys[i] = key + value;
                }

                return;
            }

            for (var i = start; i < _size; i++)
                _keys[i] += value;
        }

        private int Add(int key, object? value)
        {
            var num = Array.BinarySearch(_keys, 0, _size, key);
            if (num >= 0)
                ExceptionManager.ThrowIndexOutOfRangeCollection(nameof(key));
            return Insert(~num, key, value);
        }

        private void Clear()
        {
            Array.Clear(_keys, 0, _size);
            Array.Clear(_values, 0, _size);
            _size = 0;
        }

        private int IndexOfKey(int key)
        {
            var num = Array.BinarySearch(_keys, 0, _size, key);
            if (num < 0)
                return -1;
            return num;
        }

        private object? GetValue(int index)
        {
            if (index >= _size)
                ExceptionManager.ThrowIndexOutOfRangeCollection(nameof(index));

            return _values[index];
        }

        private void SetValue(int index, object? value)
        {
            if (index >= _size)
                ExceptionManager.ThrowIndexOutOfRangeCollection(nameof(index));

            _values[index] = value;
        }

        private void RemoveAt(int index)
        {
            if (index < 0 || index >= _size)
                ExceptionManager.ThrowIndexOutOfRangeCollection(nameof(index));

            --_size;
            if (index < _size)
            {
                Array.Copy(_keys, index + 1, _keys, index, _size - index);
                Array.Copy(_values, index + 1, _values, index, _size - index);
            }

            _keys[_size] = default;
            _values[_size] = default!;
        }

        private void EnsureCapacity(int min)
        {
            var num = _keys.Length == 0 ? 4 : _keys.Length * 2;
            if (num < min)
                num = min;
            SetCapacity(num);
        }

        private int Insert(int index, int key, object? value)
        {
            if (_size == _keys.Length)
                EnsureCapacity(_size + 1);
            if (index < _size)
            {
                Array.Copy(_keys, index, _keys, index + 1, _size - index);
                Array.Copy(_values, index, _values, index + 1, _size - index);
            }

            _keys[index] = key;
            _values[index] = value;
            ++_size;
            return index;
        }

        private void SetCapacity(int value)
        {
            if (value == _keys.Length)
                return;
            if (value < _size)
                ExceptionManager.ThrowCapacityLessThanCollection(nameof(value));

            if (value > 0)
            {
                var keyArray = new int[value];
                var objArray = new object?[value];
                if (_size > 0)
                {
                    Array.Copy(_keys, 0, keyArray, 0, _size);
                    Array.Copy(_values, 0, objArray, 0, _size);
                }

                _keys = keyArray;
                _values = objArray;
            }
            else
            {
                _keys = Default.Array<int>();
                _values = Default.Array<object?>();
            }
        }

        #endregion
    }
}