using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MugenMvvm.Extensions;
using MugenMvvm.Internal;

namespace MugenMvvm.Collections
{
    [StructLayout(LayoutKind.Auto)]
    public readonly struct ItemOrIEnumerable<T>
    {
        private readonly int _fixedCount;
        public readonly T? Item;
        public readonly IEnumerable<T>? List;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ItemOrIEnumerable(T? item, bool hasItem)
        {
            List = null;
            if (hasItem)
            {
                Item = item!;
                _fixedCount = 1;
            }
            else
            {
                Item = default!;
                _fixedCount = 0;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ItemOrIEnumerable(T[]? array)
        {
            if (array == null || array.Length == 0)
            {
                Item = default!;
                List = null;
                _fixedCount = 0;
            }
            else if (array.Length == 1)
            {
                _fixedCount = 1;
                Item = array[0];
                List = null;
            }
            else
            {
                Item = default!;
                List = array;
                _fixedCount = array.Length;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ItemOrIEnumerable(List<T>? list)
        {
            if (list == null || list.Count == 0)
            {
                Item = default!;
                List = null;
                _fixedCount = 0;
            }
            else if (list.Count == 1)
            {
                _fixedCount = 1;
                Item = list[0];
                List = null;
            }
            else
            {
                Item = default!;
                List = list;
                _fixedCount = 0;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ItemOrIEnumerable(IReadOnlyList<T>? readOnlyList)
        {
            if (readOnlyList == null || readOnlyList.Count == 0)
            {
                Item = default!;
                List = null;
                _fixedCount = 0;
            }
            else if (readOnlyList.Count == 1)
            {
                _fixedCount = 1;
                Item = readOnlyList[0];
                List = null;
            }
            else
            {
                Item = default!;
                List = readOnlyList;
                if (readOnlyList is T[] array)
                    _fixedCount = array.Length;
                else
                    _fixedCount = 0;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ItemOrIEnumerable(IEnumerable<T>? enumerable)
        {
            if (enumerable == null)
            {
                Item = default!;
                List = null;
                _fixedCount = 0;
                return;
            }

            if (enumerable is T[] array)
            {
                _fixedCount = array.Length;
                if (_fixedCount > 1)
                {
                    Item = default!;
                    List = enumerable;
                }
                else if (_fixedCount == 1)
                {
                    Item = array[0];
                    List = null;
                }
                else
                {
                    Item = default!;
                    List = null;
                }

                return;
            }

            int count;
            if (enumerable is IReadOnlyList<T> readOnlyList)
            {
                count = readOnlyList.Count;
                if (count > 1)
                {
                    _fixedCount = 0;
                    Item = default!;
                    List = enumerable;
                }
                else if (count == 1)
                {
                    _fixedCount = 1;
                    Item = readOnlyList[0];
                    List = null;
                }
                else
                {
                    _fixedCount = 0;
                    Item = default!;
                    List = null;
                }

                return;
            }

            count = enumerable.CountEx();
            if (count > 1)
            {
                _fixedCount = 0;
                Item = default!;
                List = enumerable;
            }
            else if (count == 1)
            {
                _fixedCount = 1;
                Item = enumerable.ElementAt(0);
                List = null;
            }
            else
            {
                _fixedCount = 0;
                Item = default!;
                List = null;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ItemOrIEnumerable(T? item, IEnumerable<T>? list, int fixedCount)
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
            get => _fixedCount == 0 ? List.CountEx() : _fixedCount;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ItemOrIEnumerable<T>(T? item) => new(item, item != null);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ItemOrIEnumerable<T>(T[]? items) => new(items);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ItemOrIEnumerable<T>(List<T>? items) => new(items);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ItemOrIEnumerable<TType> Cast<TType>() => new((TType?) (object?) Item!, (IEnumerable<TType>?) List, _fixedCount);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<T> AsList()
        {
            if (List != null)
                return List;
            if (Count == 0)
                return Array.Empty<T>();
            return new[] {Item!};
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T[] ToArray()
        {
            if (List != null)
                return List.ToArray();
            if (_fixedCount == 1)
                return new[] {Item!};
            return Array.Empty<T>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Enumerator GetEnumerator() => new(this);

        [StructLayout(LayoutKind.Auto)]
        public struct Enumerator : IEnumerator<T>
        {
            private readonly T _item;
            private readonly IEnumerator<T>? _enumerator;
            private readonly IReadOnlyList<T>? _readOnlyList;
            private readonly List<T>? _list;
            private readonly T[]? _array;
            private readonly int _count;
            private int _index;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Enumerator(ItemOrIEnumerable<T> itemOrList)
            {
                _index = -1;
                if (itemOrList.List == null)
                {
                    _item = itemOrList.Item!;
                    _list = null;
                    _array = null;
                    _enumerator = null;
                    _readOnlyList = null;
                    _count = itemOrList._fixedCount;
                }
                else if (itemOrList._fixedCount != 0)
                {
                    _item = default!;
                    _readOnlyList = null;
                    _list = null;
                    _enumerator = null;
                    _array = itemOrList.List as T[];
                    _count = itemOrList._fixedCount;
                }
                else
                {
                    _item = default!;
                    _array = null;
                    _list = itemOrList.List as List<T>;
                    if (_list == null)
                    {
                        _readOnlyList = itemOrList.List as IReadOnlyList<T>;
                        if (_readOnlyList == null)
                        {
                            _array = null;
                            _enumerator = itemOrList.List.GetEnumerator();
                            _count = 0;
                        }
                        else
                        {
                            _enumerator = null;
                            _count = _readOnlyList.Count;
                        }
                    }
                    else
                    {
                        _readOnlyList = null;
                        _enumerator = null;
                        _count = _list.Count;
                    }
                }
            }

            object IEnumerator.Current => Current!;

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
                    return _enumerator == null ? _item! : _enumerator.Current;
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Reset()
            {
                _index = -1;
                _enumerator?.Reset();
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext()
            {
                if (_enumerator == null)
                    return ++_index < _count;
                return _enumerator.MoveNext();
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly void Dispose() => _enumerator?.Dispose();
        }
    }
}