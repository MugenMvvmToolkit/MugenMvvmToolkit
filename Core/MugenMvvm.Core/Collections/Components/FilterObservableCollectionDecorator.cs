using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Components;
using MugenMvvm.Constants;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Collections.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Collections.Components
{
    public sealed class FilterObservableCollectionDecorator<T> : AttachableComponentBase<IObservableCollection<T>>, IDecoratorObservableCollectionComponent<T>, IEnumerable<T>, IHasPriority
    {
        #region Fields

        private Func<T, bool>? _filter;

        private int[] _keys;
        private int _size;
        private T[] _values;

        #endregion

        #region Constructors

        public FilterObservableCollectionDecorator()
        {
            _keys = Default.EmptyArray<int>();
            _values = Default.EmptyArray<T>();
            _size = 0;
        }

        #endregion

        #region Properties

        public Func<T, bool>? Filter
        {
            get => _filter;
            set
            {
                if (_filter == value)
                    return;
                _filter = value;
                UpdateFilterInternal(value);
            }
        }

        public int Priority { get; set; } = CollectionComponentPriority.FilterDecorator;

        private bool HasFilter => Filter != null && IsAttached;

        #endregion

        #region Implementation of interfaces

        IEnumerable<T> IDecoratorObservableCollectionComponent<T>.DecorateItems(IEnumerable<T> items)
        {
            var filter = Filter;
            if (filter == null)
                return items;

            return items.Where(filter);
        }

        bool IDecoratorObservableCollectionComponent<T>.OnItemChanged(ref T item, ref int index, ref object? args)
        {
            if (!HasFilter)
                return true;

            var decoratorManager = Owner.DecoratorManager;
            var filterIndex = IndexOfKey(index);
            if (FilterInternal(item))
            {
                if (filterIndex == -1)
                {
                    index = Add(index, item);
                    decoratorManager.OnAdded(this, item, index);
                }
                else
                    index = filterIndex;

                return true;
            }

            if (filterIndex != -1)
            {
                RemoveAt(filterIndex);
                decoratorManager.OnRemoved(this, item, filterIndex);
            }

            return false;
        }

        bool IDecoratorObservableCollectionComponent<T>.OnAdded(ref T item, ref int index)
        {
            if (!HasFilter)
                return true;

            UpdateIndexes(index, 1);
            if (!FilterInternal(item))
                return false;

            index = Add(index, item);
            return true;
        }

        bool IDecoratorObservableCollectionComponent<T>.OnReplaced(ref T oldItem, ref T newItem, ref int index)
        {
            if (!HasFilter)
                return true;

            var decoratorManager = Owner.DecoratorManager;
            var filterIndex = IndexOfKey(index);
            if (filterIndex == -1)
            {
                if (FilterInternal(newItem))
                    decoratorManager.OnAdded(this, newItem, Add(index, newItem));

                return false;
            }

            if (FilterInternal(newItem))
            {
                oldItem = GetValue(filterIndex);
                SetValue(filterIndex, newItem);
                index = filterIndex;
                return true;
            }

            var oldValue = GetValue(filterIndex);
            RemoveAt(filterIndex);
            decoratorManager.OnRemoved(this, oldValue, filterIndex);
            return false;
        }

        bool IDecoratorObservableCollectionComponent<T>.OnMoved(ref T item, ref int oldIndex, ref int newIndex)
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

        bool IDecoratorObservableCollectionComponent<T>.OnRemoved(ref T item, ref int index)
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

        bool IDecoratorObservableCollectionComponent<T>.OnReset(ref IEnumerable<T> items)
        {
            var filter = Filter;
            if (filter == null)
                return true;

            Clear();
            UpdateItems(items, filter);
            items = items.Where(filter);
            return true;
        }

        bool IDecoratorObservableCollectionComponent<T>.OnCleared()
        {
            Clear();
            return true;
        }

        public IEnumerator<T> GetEnumerator()
        {
            for (var i = 0; i < _size; i++)
                yield return _values[i];
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region Methods

        protected override void OnAttachedInternal(IObservableCollection<T> owner, IReadOnlyMetadataContext? metadata)
        {
            UpdateFilter();
        }

        protected override void OnDetachedInternal(IObservableCollection<T> owner, IReadOnlyMetadataContext? metadata)
        {
            Clear();
        }

        public void UpdateFilter()
        {
            UpdateFilterInternal(Filter);
        }

        private void UpdateFilterInternal(Func<T, bool>? filter)
        {
            if (!IsAttached)
                return;

            var decoratorManager = Owner.DecoratorManager;
            using (decoratorManager.Lock())
            {
                Clear();
                if (filter != null)
                    UpdateItems(decoratorManager.DecorateItems(this), filter);
                decoratorManager.OnReset(this, this);
            }
        }

        private void UpdateItems(IEnumerable<T> items, Func<T, bool> filter)
        {
            var index = 0;
            foreach (var item in items)
            {
                if (filter(item))
                    Add(index, item);
                ++index;
            }
        }

        private bool FilterInternal(T value)
        {
            return Filter?.Invoke(value) ?? true;
        }

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

        private int Add(int key, T value)
        {
            var num = Array.BinarySearch(_keys, 0, _size, key);
            if (num >= 0)
                throw new InvalidOperationException();
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

        private T GetValue(int index)
        {
            if (index >= _size)
                ExceptionManager.ThrowIntOutOfRangeCollection("index");

            return _values[index];
        }

        private void SetValue(int index, T value)
        {
            if (index >= _size)
                ExceptionManager.ThrowIntOutOfRangeCollection("index");

            _values[index] = value;
        }

        private void RemoveAt(int index)
        {
            if (index < 0 || index >= _size)
                ExceptionManager.ThrowIntOutOfRangeCollection("index");

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

        private int Insert(int index, int key, T value)
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
                ExceptionManager.ThrowCapacityLessThanCollection("Capacity");

            if (value > 0)
            {
                var keyArray = new int[value];
                var objArray = new T[value];
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
                _keys = Default.EmptyArray<int>();
                _values = Default.EmptyArray<T>();
            }
        }

        #endregion
    }
}