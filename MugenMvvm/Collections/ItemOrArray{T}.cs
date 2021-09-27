using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MugenMvvm.Constants;
using MugenMvvm.Extensions;
using MugenMvvm.Internal;

namespace MugenMvvm.Collections
{
    [StructLayout(LayoutKind.Auto)]
    public readonly struct ItemOrArray<T> : IReadOnlyList<T>
    {
        public readonly T? Item;
        public readonly T[]? List;
        public readonly int Count;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ItemOrArray(T? item)
        {
            Item = item;
            List = null;
            Count = 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ItemOrArray(T? item, T[]? list, int count)
        {
            Item = item!;
            List = list;
            Count = count;
        }

        public bool IsEmpty
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Count == 0 && List == null;
        }

        public bool HasItem
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Count == 1 && List == null;
        }

        [IndexerName(InternalConstant.CustomIndexerName)]
        public T this[int index]
        {
            get
            {
                if (List != null)
                    return List[index];
                if ((uint)index < (uint)Count)
                    return Item!;
                ExceptionManager.ThrowIndexOutOfRangeCollection(nameof(index));
                return default!;
            }
        }

        int IReadOnlyCollection<T>.Count => Count;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ItemOrArray<T>(T? item) => ItemOrArray.FromItem(item, item != null);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ItemOrArray<T>(T[]? items) => ItemOrArray.FromList(items);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ItemOrIEnumerable<T>(ItemOrArray<T> itemOrList) => new(itemOrList.Item!, itemOrList.List, itemOrList.Count);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ItemOrIReadOnlyCollection<T>(ItemOrArray<T> itemOrList) => new(itemOrList.Item!, itemOrList.List, itemOrList.Count);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ItemOrIReadOnlyList<T>(ItemOrArray<T> itemOrList) => new(itemOrList.Item!, itemOrList.List, itemOrList.Count);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<T> AsEnumerable()
        {
            if (List != null)
                return List;
            if (Count == 0)
                return Default.Enumerable<T>();
            return Default.SingleItemEnumerable(Item!);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T[] AsList()
        {
            if (List != null)
                return List;
            if (Count == 0)
                return Array.Empty<T>();
            return new[] { Item! };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public List<T> ToList()
        {
            if (List != null)
                return new List<T>(List);
            if (Count == 0)
                return new List<T>();
            return new List<T> { Item! };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T[] ToArray()
        {
            if (List != null)
                return List.ToArray();
            if (Count == 0)
                return Array.Empty<T>();
            return new[] { Item! };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Enumerator GetEnumerator() => new(this);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private IEnumerator<T> GetEnumeratorRef()
        {
            if (List != null)
                return ((IEnumerable<T>)List).GetEnumerator();
            if (Count == 0)
                return Default.Enumerator<T>();
            return Default.SingleItemEnumerator(Item!);
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumeratorRef();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumeratorRef();

        [StructLayout(LayoutKind.Auto)]
        public struct Enumerator
        {
            private readonly T _item;
            private readonly T[]? _array;
            private readonly int _count;
            private int _index;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal Enumerator(ItemOrArray<T> itemOrList)
            {
                _index = -1;
                _count = itemOrList.Count;
                _item = itemOrList.Item!;
                _array = itemOrList.List;
            }

            public readonly T Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get
                {
                    if (_array == null)
                        return _item;
                    return _array[_index];
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Reset() => _index = -1;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext() => ++_index < _count;
        }
    }
}