using System;
using System.Collections.Generic;

namespace MugenMvvm.Collections
{
    public class ArrayListLight<T>
    {
        #region Fields

        private T[] _items;
        private int _size;
        private const int DefaultCapacity = 4;

        #endregion

        #region Constructors

        public ArrayListLight()
        {
            _items = Default.EmptyArray<T>();
        }

        public ArrayListLight(uint capacity)
        {
            if (capacity == 0)
                _items = Default.EmptyArray<T>();
            else
                _items = new T[capacity];
        }

        public ArrayListLight(IEnumerable<T> collection)
        {
            Should.NotBeNull(collection, nameof(collection));
            if (collection is ICollection<T> items)
            {
                var count = items.Count;
                if (count == 0)
                    _items = Default.EmptyArray<T>();
                else
                {
                    _items = new T[count];
                    items.CopyTo(_items, 0);
                    _size = count;
                }
            }
            else
            {
                _size = 0;
                _items = Default.EmptyArray<T>();
                foreach (var obj in collection)
                    Add(obj);
            }
        }

        #endregion

        #region Properties

        private uint Capacity
        {
            get => (uint)_items.Length;
            set
            {
                if (value < _size)
                    throw ExceptionManager.CapacityLessThanCollection(nameof(Capacity));
                if (value == _items.Length)
                    return;
                if (value > 0)
                {
                    var objArray = new T[value];
                    if (_size > 0)
                        Array.Copy(_items, 0, objArray, 0, _size);
                    _items = objArray;
                }
                else
                    _items = Default.EmptyArray<T>();
            }
        }

        #endregion

        #region Methods        

        public T[] GetItemsWithLock(out int size)
        {
            lock (this)
            {
                return GetItems(out size);
            }
        }

        public void AddWithLock(T item)
        {
            lock (this)
            {
                Add(item);
            }
        }

        public bool ContainsWithLock(T item)
        {
            var items = GetItemsWithLock(out var size);
            return ContainsInternal(items, size, item);
        }

        public bool RemoveWithLock(T item)
        {
            lock (this)
            {
                return Remove(item);
            }
        }

        public void ClearWithLock()
        {
            lock (this)
            {
                Clear();
            }
        }

        public T[] GetItems(out int size)
        {
            size = _size;
            return _items;
        }

        public void Add(T item)
        {
            if (_size == _items.Length)
                EnsureCapacity((uint)_size + 1);
            _items[_size++] = item;
        }

        public bool Contains(T item)
        {
            var items = GetItems(out var size);
            return ContainsInternal(items, size, item);
        }

        public bool Remove(T item)
        {
            var index = IndexOfInternal(item);
            if (index < 0)
                return false;
            RemoveAtInternal(index);
            return true;
        }

        public void Clear()
        {
            if (_size > 0)
            {
                Array.Clear(_items, 0, _size);
                _size = 0;
            }
        }

        public T[] ToArray()
        {
            if (_size == 0)
                return Default.EmptyArray<T>();
            var result = new T[_size];
            for (int i = 0; i < result.Length; i++)
                result[i] = _items[i];

            return result;
        }

        private static bool ContainsInternal(T[] items, int size, T item)
        {
            if (item == null)
            {
                for (var index = 0; index < size; ++index)
                {
                    if (items[index] == null)
                        return true;
                }

                return false;
            }

            var equalityComparer = EqualityComparer<T>.Default;
            for (var index = 0; index < size; ++index)
            {
                if (equalityComparer.Equals(items[index], item))
                    return true;
            }

            return false;
        }

        private void RemoveAtInternal(int index)
        {
            if (index > _size)
                throw ExceptionManager.IndexOutOfRangeCollection("index");
            --_size;
            if (index < _size)
                Array.Copy(_items, index + 1, _items, index, _size - index);
            _items[_size] = default!;
        }

        private int IndexOfInternal(T item)
        {
            return Array.IndexOf(_items, item, 0, _size);
        }

        private void EnsureCapacity(uint min)
        {
            if (_items.Length >= min)
                return;
            var num = (uint)(_items.Length == 0 ? DefaultCapacity : _items.Length * 2);
            if (num > uint.MaxValue)
                num = 2146435071;
            if (num < min)
                num = min;
            Capacity = num;
        }

        #endregion
    }
}