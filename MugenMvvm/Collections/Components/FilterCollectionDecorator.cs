﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Components;
using MugenMvvm.Constants;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Collections.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Collections.Components
{
    public class FilterCollectionDecorator<T> : AttachableComponentBase<ICollection>, ICollectionDecorator, IReadOnlyCollection<object?>, IHasPriority
    {
        private readonly Func<object?, bool> _internalFilter;
        private ICollectionDecoratorManagerComponent? _decoratorManager;
        private Func<T, bool>? _filter;

        private int[] _keys;
        private int _size;
        private object?[] _values;

        public FilterCollectionDecorator(Func<T, bool>? filter = null, int priority = CollectionComponentPriority.FilterDecorator)
        {
            _filter = filter;
            _keys = Array.Empty<int>();
            _values = Array.Empty<object?>();
            _size = 0;
            _internalFilter = FilterInternal;
            Priority = priority;
        }

        public Func<T, bool>? Filter
        {
            get => _filter;
            set
            {
                if (_filter != value)
                    UpdateFilterInternal(value, true);
            }
        }

        public int Priority { get; set; }

        private bool HasFilter => _filter != null && _decoratorManager != null;

        int IReadOnlyCollection<object?>.Count => _size;

        public void UpdateFilter() => UpdateFilterInternal(null, false);

        public IEnumerator<object?> GetEnumerator()
        {
            for (var i = 0; i < _size; i++)
                yield return _values[i];
        }

        protected override void OnAttached(ICollection owner, IReadOnlyMetadataContext? metadata) => _decoratorManager = CollectionDecoratorManager.GetOrAdd(owner);

        protected override void OnDetached(ICollection owner, IReadOnlyMetadataContext? metadata)
        {
            Clear();
            _decoratorManager = null;
        }

        private void UpdateFilterInternal(Func<T, bool>? filter, bool setFilter)
        {
            using var _ = OwnerOptional.TryLock();
            if (_decoratorManager == null)
            {
                if (setFilter)
                    _filter = filter;
                return;
            }

            if (setFilter)
                _filter = filter;
            Clear();
            if (HasFilter)
                UpdateItems(_decoratorManager.Decorate(Owner, this));
            _decoratorManager.OnReset(Owner, this, this);
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

        private bool FilterInternal(object? value) => _filter == null || value is not T v || _filter(v);

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
                _keys = Array.Empty<int>();
                _values = Array.Empty<object?>();
            }
        }

        IEnumerable<object?> ICollectionDecorator.Decorate(ICollection collection, IEnumerable<object?> items) => HasFilter ? items.Where(_internalFilter) : items;

        bool ICollectionDecorator.OnChanged(ICollection collection, ref object? item, ref int index, ref object? args)
        {
            if (!HasFilter)
                return true;

            var filterIndex = IndexOfKey(index);
            if (FilterInternal(item))
            {
                if (filterIndex == -1)
                {
                    index = Add(index, item);
                    _decoratorManager!.OnAdded(collection, this, item, index);
                }
                else
                    index = filterIndex;

                return true;
            }

            if (filterIndex != -1)
            {
                RemoveAt(filterIndex);
                _decoratorManager!.OnRemoved(collection, this, item, filterIndex);
            }

            return false;
        }

        bool ICollectionDecorator.OnAdded(ICollection collection, ref object? item, ref int index)
        {
            if (!HasFilter)
                return true;

            UpdateIndexes(index, 1);
            if (!FilterInternal(item))
                return false;

            index = Add(index, item);
            return true;
        }

        bool ICollectionDecorator.OnReplaced(ICollection collection, ref object? oldItem, ref object? newItem, ref int index)
        {
            if (!HasFilter)
                return true;

            var filterIndex = IndexOfKey(index);
            if (filterIndex == -1)
            {
                if (FilterInternal(newItem))
                    _decoratorManager!.OnAdded(collection, this, newItem, Add(index, newItem));

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
            _decoratorManager!.OnRemoved(collection, this, oldValue, filterIndex);
            return false;
        }

        bool ICollectionDecorator.OnMoved(ICollection collection, ref object? item, ref int oldIndex, ref int newIndex)
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

        bool ICollectionDecorator.OnRemoved(ICollection collection, ref object? item, ref int index)
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

        bool ICollectionDecorator.OnReset(ICollection collection, ref IEnumerable<object?>? items)
        {
            if (!HasFilter)
                return true;

            Clear();
            if (items != null)
            {
                UpdateItems(items);
                items = this;
            }

            return true;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}