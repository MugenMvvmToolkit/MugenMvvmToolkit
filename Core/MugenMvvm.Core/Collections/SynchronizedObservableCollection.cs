//using System;
//using System.Collections;
//using System.Collections.Generic;
//using MugenMvvm.Interfaces.Collections;
//using MugenMvvm.Interfaces.Metadata;
//using MugenMvvm.Interfaces.Models;
//
//namespace MugenMvvm.Collections
//{
//
//    [Serializable]
//    public class FilterObservableCollectionDecorator<TItem> : IObservableCollectionDecorator<TItem>
//    {
//        #region Fields
//
//        private int[] _keys;
//        private int _size;
//        private TItem[] _values;
//
//        public const int DefaultPriority = 1;
//
//        #endregion
//
//        #region Constructors
//
//        public FilterObservableCollectionDecorator(int priority = DefaultPriority)
//        {
//            Priority = priority;
//            _keys = Default.EmptyArray<int>();
//            _values = Default.EmptyArray<TItem>();
//            _size = 0;
//        }
//
//        #endregion
//
//        #region Properties
//
//        public int Priority { get; }
//
//        private int Capacity
//        {
//            get => _keys.Length;
//            set
//            {
//                if (value == _keys.Length)
//                    return;
//                if (value < _size)
//                    throw ExceptionManager.CapacityLessThanCollection("Capacity");
//                if (value > 0)
//                {
//                    var keyArray = new int[value];
//                    var objArray = new TItem[value];
//                    if (_size > 0)
//                    {
//                        Array.Copy(_keys, 0, keyArray, 0, _size);
//                        Array.Copy(_values, 0, objArray, 0, _size);
//                    }
//
//                    _keys = keyArray;
//                    _values = objArray;
//                }
//                else
//                {
//                    _keys = Default.EmptyArray<int>();
//                    _values = Default.EmptyArray<TItem>();
//                }
//            }
//        }
//
//        #endregion
//
//        #region Implementation of interfaces
//
//        public bool OnAdded(IDecorableObservableCollection<TItem> collection, ref TItem item, ref int index)
//        {
//            UpdateFilterItems(index, 1);
//            if (!Filter(item))
//                return false;
//
//            index = Add(index, item);
//            return true;
//        }
//
//        public bool OnReplaced(IDecorableObservableCollection<TItem> collection, ref TItem oldItem, ref TItem newItem, ref int index)
//        {
//            var filterIndex = IndexOfKey(index);
//            if (filterIndex == -1)
//                return false;
//
//            if (Filter(newItem))
//            {
//                oldItem = GetValue(filterIndex);
//                index = filterIndex;
//                return true;
//            }
//
//            var oldValue = GetValue(filterIndex);
//            RemoveAt(filterIndex);
//            collection.RaiseRemoved(this, oldValue, filterIndex);
//            return false;
//        }
//
//        public bool OnMoved(IDecorableObservableCollection<TItem> collection, ref TItem item, ref int oldIndex, ref int newIndex)
//        {
//            throw new NotImplementedException();
//        }
//
//        public bool OnRemoved(IDecorableObservableCollection<TItem> collection, ref TItem item, ref int index)
//        {
//            var filterIndex = IndexOfKey(index);
//            UpdateFilterItems(index, -1);
//            if (filterIndex == -1)
//                return false;
//
//            RemoveAt(filterIndex);
//            index = filterIndex;
//            return true;
//        }
//
//        public bool OnCleared(IDecorableObservableCollection<TItem> collection)
//        {
//            Clear();
//            return true;
//        }
//
//        #endregion
//
//        #region Methods
//
//        private bool Filter(TItem value)
//        {
//            return false;
//        }
//
//        private void UpdateFilterItems(int index, int value)
//        {
//            if (_size == 0)
//                return;
//
//            int start = IndexOfKey(index);
//            if (start == -1)
//            {
//                if (_keys[_size - 1] < index)
//                    return;
//                for (int i = 0; i < _size; i++)
//                {
//                    int key = _keys[i];
//                    if (key < index)
//                        continue;
//                    _keys[i] = key + value;
//                }
//                return;
//            }
//            for (int i = start; i < _size; i++)
//                _keys[i] += value;
//        }
//
//        private int Add(int key, TItem value)
//        {
//            var num = Array.BinarySearch(_keys, 0, _size, key);
//            if (num >= 0)
//                throw new InvalidOperationException();
//            return Insert(~num, key, value);
//        }
//
//        private void Clear()
//        {
//            Array.Clear(_keys, 0, _size);
//            Array.Clear(_values, 0, _size);
//            _size = 0;
//        }
//
//        private int IndexOfKey(int key)
//        {
//            var num = Array.BinarySearch(_keys, 0, _size, key);
//            if (num < 0)
//                return -1;
//            return num;
//        }
//
//        private int IndexOfValue(TItem value)
//        {
//            return Array.IndexOf(_values, value, 0, _size);
//        }
//
//        private int GetKey(int index)
//        {
//            if (index >= _size)
//                throw ExceptionManager.IntOutOfRangeCollection("index");
//            return _keys[index];
//        }
//
//        private TItem GetValue(int index)
//        {
//            if (index >= _size)
//                throw ExceptionManager.IntOutOfRangeCollection("index");
//            return _values[index];
//        }
//
//        private void RemoveAt(int index)
//        {
//            if (index < 0 || index >= _size)
//                throw ExceptionManager.IntOutOfRangeCollection("index");
//            --_size;
//            if (index < _size)
//            {
//                Array.Copy(_keys, index + 1, _keys, index, _size - index);
//                Array.Copy(_values, index + 1, _values, index, _size - index);
//            }
//
//            _keys[_size] = default;
//            _values[_size] = default;
//        }
//
//        private void EnsureCapacity(int min)
//        {
//            var num = _keys.Length == 0 ? 4 : _keys.Length * 2;
//            if (num < min)
//                num = min;
//            Capacity = num;
//        }
//
//        private int Insert(int index, int key, TItem value)
//        {
//            if (_size == _keys.Length)
//                EnsureCapacity(_size + 1);
//            if (index < _size)
//            {
//                Array.Copy(_keys, index, _keys, index + 1, _size - index);
//                Array.Copy(_values, index, _values, index + 1, _size - index);
//            }
//
//            _keys[index] = key;
//            _values[index] = value;
//            ++_size;
//            return index;
//        }
//
//        #endregion
//    }
//
//
//    [Serializable]
//    public class SynchronizedObservableCollection<T> : SynchronizedObservableCollectionBase<T, List<T>>
//    {
//        #region Constructors
//
//        public SynchronizedObservableCollection(IComponentCollection<IObservableCollectionChangedListener<T>>? listeners = null)
//            : base(new List<T>(), listeners)
//        {
//        }
//
//        public SynchronizedObservableCollection(IEnumerable<T> items, IComponentCollection<IObservableCollectionChangedListener<T>>? listeners = null)
//            : base(new List<T>(items), listeners)
//        {
//        }
//
//        #endregion
//    }
//}