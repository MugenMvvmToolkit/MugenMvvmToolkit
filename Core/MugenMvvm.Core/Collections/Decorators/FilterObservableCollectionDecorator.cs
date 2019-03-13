using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Collections.Decorators
{
    public class FilterObservableCollectionDecorator<T> : IObservableCollectionDecorator<T>, IEnumerable<T>
    {
        #region Fields

        private IObservableCollectionDecoratorManager<T> _decoratorManager;
        private Func<T, bool>? _filter;

        private int[] _keys;
        private int _size;
        private T[] _values;

        public const int DefaultPriority = 1;

        #endregion

        #region Constructors

        public FilterObservableCollectionDecorator(int priority = DefaultPriority)
        {
            Priority = priority;
            _keys = Default.EmptyArray<int>();
            _values = Default.EmptyArray<T>();
            _size = 0;
        }

        #endregion

        #region Properties

        public int Priority { get; }

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

        private int Capacity
        {
            set
            {
                if (value == _keys.Length)
                    return;
                if (value < _size)
                    throw ExceptionManager.CapacityLessThanCollection("Capacity");
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
        }

        private bool HasFilter => Filter != null;

        #endregion

        #region Implementation of interfaces

        public IEnumerator<T> GetEnumerator()
        {
            for (var i = 0; i < _size; i++)
                yield return _values[i];
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerable<T> IObservableCollectionDecorator<T>.DecorateItems(IEnumerable<T> items)
        {
            var filter = Filter;
            if (filter == null)
                return items;

            return items.Where(filter);
        }

        bool IObservableCollectionDecorator<T>.OnAdded(ref T item, ref int index)
        {
            if (!HasFilter)
                return true;

            UpdateFilterItems(index, 1);
            if (!FilterInternal(item))
                return false;

            index = Add(index, item);
            return true;
        }

        bool IObservableCollectionDecorator<T>.OnReplaced(ref T oldItem, ref T newItem, ref int index)
        {
            if (!HasFilter)
                return true;

            var filterIndex = IndexOfKey(index);
            if (filterIndex == -1)
                return false;

            if (FilterInternal(newItem))
            {
                oldItem = GetValue(filterIndex);
                index = filterIndex;
                return true;
            }

            var oldValue = GetValue(filterIndex);
            RemoveAt(filterIndex);
            _decoratorManager.OnRemoved(this, oldValue, filterIndex);
            return false;
        }

        bool IObservableCollectionDecorator<T>.OnMoved(ref T item, ref int oldIndex, ref int newIndex)
        {
            if (!HasFilter)
                return true;

            throw new NotImplementedException();
        }

        bool IObservableCollectionDecorator<T>.OnRemoved(ref T item, ref int index)
        {
            if (!HasFilter)
                return true;

            var filterIndex = IndexOfKey(index);
            UpdateFilterItems(index, -1);
            if (filterIndex == -1)
                return false;

            RemoveAt(filterIndex);
            index = filterIndex;
            return true;
        }

        bool IObservableCollectionDecorator<T>.OnReset(ref IEnumerable<T> items)
        {
            var filter = Filter;
            if (filter == null)
                return true;

            Clear();
            UpdateItems(items, filter);
            items = items.Where(filter);
            return true;
        }

        bool IObservableCollectionDecorator<T>.OnCleared()
        {
            Clear();
            return true;
        }

        void IAttachableComponent<IObservableCollectionDecoratorManager<T>>.OnAttached(IObservableCollectionDecoratorManager<T> owner)
        {
            _decoratorManager = owner;
            UpdateFilter();
        }

        #endregion

        #region Methods

        public void UpdateFilter()
        {
            UpdateFilterInternal(Filter);
        }

        private void UpdateFilterInternal(Func<T, bool>? filter)
        {
            using (_decoratorManager.Lock())
            {
                Clear();
                if (filter != null)
                    UpdateItems(_decoratorManager.DecorateItems(this), filter);
                _decoratorManager.OnReset(this, this);
            }
        }

        private void UpdateItems(IEnumerable<T> items, Func<T, bool> filter)
        {
            var index = 0;
            foreach (var item in items)
            {
                if (filter(item))
                    Add(index++, item);
            }
        }

        private bool FilterInternal(T value)
        {
            return Filter?.Invoke(value) ?? true;
        }

        private void UpdateFilterItems(int index, int value)
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
                throw ExceptionManager.IntOutOfRangeCollection("index");
            return _values[index];
        }

        private void RemoveAt(int index)
        {
            if (index < 0 || index >= _size)
                throw ExceptionManager.IntOutOfRangeCollection("index");
            --_size;
            if (index < _size)
            {
                Array.Copy(_keys, index + 1, _keys, index, _size - index);
                Array.Copy(_values, index + 1, _values, index, _size - index);
            }

            _keys[_size] = default;
            _values[_size] = default;
        }

        private void EnsureCapacity(int min)
        {
            var num = _keys.Length == 0 ? 4 : _keys.Length * 2;
            if (num < min)
                num = min;
            Capacity = num;
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

        #endregion
    }
}