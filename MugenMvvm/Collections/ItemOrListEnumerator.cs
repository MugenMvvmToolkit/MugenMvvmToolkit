using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MugenMvvm.Collections
{
    [StructLayout(LayoutKind.Auto)]
    public struct ItemOrListEnumerator<T> : IEnumerator<T>
    {
        private readonly T _item;
        private readonly IEnumerator<T>? _enumerator;
        private readonly List<T>? _list;
        private readonly T[]? _array;
        private readonly int _count;
        private int _index;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ItemOrListEnumerator(ItemOrIEnumerable<T> itemOrList)
        {
            _index = -1;
            if (itemOrList.List == null)
            {
                _item = itemOrList.Item!;
                _list = null;
                _array = null;
                _enumerator = null;
                _count = itemOrList.FixedCount;
            }
            else if (itemOrList.FixedCount != 0)
            {
                _item = default!;
                _list = null;
                _enumerator = null;
                _array = itemOrList.List as T[];
                _count = itemOrList.FixedCount;
            }
            else
            {
                _item = default!;
                _array = null;
                _list = itemOrList.List as List<T>;
                if (_list == null)
                {
                    _array = null;
                    _enumerator = itemOrList.List.GetEnumerator();
                    _count = 0;
                }
                else
                {
                    _enumerator = null;
                    _count = _list.Count;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ItemOrListEnumerator(ItemOrIReadOnlyCollection<T> itemOrList)
        {
            _index = -1;
            if (itemOrList.List == null)
            {
                _item = itemOrList.Item!;
                _list = null;
                _array = null;
                _enumerator = null;
                _count = itemOrList.FixedCount;
            }
            else if (itemOrList.FixedCount != 0)
            {
                _item = default!;
                _list = null;
                _enumerator = null;
                _array = itemOrList.List as T[];
                _count = itemOrList.FixedCount;
            }
            else
            {
                _item = default!;
                _array = null;
                _list = itemOrList.List as List<T>;
                if (_list == null)
                {
                    _array = null;
                    _enumerator = itemOrList.List.GetEnumerator();
                    _count = 0;
                }
                else
                {
                    _enumerator = null;
                    _count = _list.Count;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ItemOrListEnumerator(ItemOrIReadOnlyList<T> itemOrList)
        {
            _index = -1;
            if (itemOrList.List == null)
            {
                _item = itemOrList.Item!;
                _list = null;
                _array = null;
                _enumerator = null;
                _count = itemOrList.FixedCount;
            }
            else if (itemOrList.FixedCount != 0)
            {
                _item = default!;
                _list = null;
                _enumerator = null;
                _array = itemOrList.List as T[];
                _count = itemOrList.FixedCount;
            }
            else
            {
                _item = default!;
                _array = null;
                _list = itemOrList.List as List<T>;
                if (_list == null)
                {
                    _array = null;
                    _enumerator = itemOrList.List.GetEnumerator();
                    _count = 0;
                }
                else
                {
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