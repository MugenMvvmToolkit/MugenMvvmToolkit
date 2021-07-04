﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MugenMvvm.Constants;
using MugenMvvm.Internal;

namespace MugenMvvm.Collections
{
    [StructLayout(LayoutKind.Auto)]
    public readonly struct ItemOrIReadOnlyList<T> : IReadOnlyList<T>
    {
        private readonly int _fixedCount;
        public readonly T? Item;
        public readonly IReadOnlyList<T>? List;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ItemOrIReadOnlyList(T? item)
        {
            Item = item;
            List = null;
            _fixedCount = 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ItemOrIReadOnlyList(T? item, IReadOnlyList<T>? list, int fixedCount)
        {
            Item = item!;
            List = list;
            _fixedCount = fixedCount;
        }

        public bool IsEmpty
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _fixedCount == 0 && List == null;
        }

        public bool HasItem
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _fixedCount == 1 && List == null;
        }

        public int Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (_fixedCount != 0)
                    return _fixedCount;
                if (List == null)
                    return 0;
                return List.Count;
            }
        }

        [IndexerName(InternalConstant.CustomIndexerName)]
        public T this[int index]
        {
            get
            {
                if (List != null)
                {
                    if (_fixedCount != 0)
                        return ((T[])List)[index];
                    return List[index];
                }

                if ((uint)index < (uint)_fixedCount)
                    return Item!;
                ExceptionManager.ThrowIndexOutOfRangeCollection(nameof(index));
                return default!;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ItemOrIReadOnlyList<T>(T? item) => ItemOrIReadOnlyList.FromItem(item, item != null);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ItemOrIReadOnlyList<T>(T[]? items) => ItemOrIReadOnlyList.FromList(items);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ItemOrIReadOnlyList<T>(List<T>? items) => ItemOrIReadOnlyList.FromList<List<T>, T>(items);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ItemOrIEnumerable<T>(ItemOrIReadOnlyList<T> itemOrList) => new(itemOrList.Item!, itemOrList.List, itemOrList._fixedCount);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<T> AsEnumerable()
        {
            if (List != null)
                return List;
            if (_fixedCount == 0)
                return Default.EmptyEnumerable<T>();
            return Default.SingleItemEnumerable(Item!);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IReadOnlyList<T> AsList()
        {
            if (List != null)
                return List;
            if (_fixedCount == 0)
                return Array.Empty<T>();
            return new[] { Item! };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public List<T> ToList()
        {
            if (List != null)
                return new List<T>(List);
            if (_fixedCount == 0)
                return new List<T>();
            return new List<T> { Item! };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T[] ToArray()
        {
            if (List != null)
                return List.ToArray();
            if (_fixedCount == 0)
                return Array.Empty<T>();
            return new[] { Item! };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Enumerator GetEnumerator() => new(this);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private IEnumerator<T> GetEnumeratorRef()
        {
            if (List != null)
                return List.GetEnumerator();
            if (_fixedCount == 0)
                return Default.EmptyEnumerator<T>();
            return Default.SingleItemEnumerator(Item!);
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumeratorRef();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumeratorRef();

        [StructLayout(LayoutKind.Auto)]
        public struct Enumerator
        {
            private readonly T _item;
            private readonly IReadOnlyList<T>? _readOnlyList;
            private readonly List<T>? _list;
            private readonly T[]? _array;
            private readonly int _count;
            private int _index;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal Enumerator(ItemOrIReadOnlyList<T> itemOrList)
            {
                _index = -1;
                if (itemOrList.List == null)
                {
                    _item = itemOrList.Item!;
                    _list = null;
                    _array = null;
                    _readOnlyList = null;
                    _count = itemOrList._fixedCount;
                }
                else if (itemOrList._fixedCount != 0)
                {
                    _item = default!;
                    _readOnlyList = null;
                    _list = null;
                    _array = (T[])itemOrList.List;
                    _count = itemOrList._fixedCount;
                }
                else
                {
                    _item = default!;
                    _array = null;
                    _list = itemOrList.List as List<T>;
                    if (_list == null)
                    {
                        _readOnlyList = itemOrList.List;
                        _count = _readOnlyList.Count;
                    }
                    else
                    {
                        _readOnlyList = null;
                        _count = _list.Count;
                    }
                }
            }

            public readonly T Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get
                {
                    if (_array != null)
                        return _array[_index];
                    if (_list != null)
                        return _list[_index];
                    if (_readOnlyList != null)
                        return _readOnlyList[_index];
                    return _item;
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Reset() => _index = -1;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext() => ++_index < _count;
        }
    }
}