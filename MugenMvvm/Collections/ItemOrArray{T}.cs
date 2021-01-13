using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MugenMvvm.Attributes;
using MugenMvvm.Constants;
using MugenMvvm.Extensions;
using MugenMvvm.Internal;

namespace MugenMvvm.Collections
{
    [StructLayout(LayoutKind.Auto)]
    public readonly struct ItemOrArray<T>
    {
        #region Fields

        public readonly T? Item;
        public readonly T[]? List;

        #endregion

        #region Constructors

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ItemOrArray(T? item, bool hasItem)
        {
            List = null;
            if (hasItem)
            {
                Item = item!;
                Count = 1;
            }
            else
            {
                Item = default!;
                Count = 0;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ItemOrArray(T[]? array)
        {
            if (array == null || array.Length == 0)
            {
                Item = default!;
                List = null;
                Count = 0;
            }
            else if (array.Length == 1)
            {
                Count = 1;
                Item = array[0];
                List = null;
            }
            else
            {
                Item = default!;
                List = array;
                Count = array.Length;
            }
        }

        [Preserve]
        internal ItemOrArray(T? item, T[]? list, int count)
        {
            Item = item!;
            List = list;
            Count = count;
        }

        #endregion

        #region Properties

        public bool IsEmpty
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Count == 0;
        }

        public bool HasItem
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Count == 1 && List == null;
        }

        public int Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get;
        }

        [IndexerName(InternalConstant.CustomIndexerName)]
        public T this[int index]
        {
            get
            {
                if (List != null)
                    return List[index];
                if ((uint) index < (uint) Count)
                    return Item!;
                ExceptionManager.ThrowIndexOutOfRangeCollection(nameof(index));
                return default!;
            }
        }

        #endregion

        #region Methods

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ItemOrArray<T>(T? item) => new(item, item != null);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ItemOrArray<T>(T[]? items) => new(items);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ItemOrIEnumerable<T>(ItemOrArray<T> itemOrList) => new(itemOrList.Item!, itemOrList.List, itemOrList.Count);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ItemOrIReadOnlyList<T>(ItemOrArray<T> itemOrList) => new(itemOrList.Item!, itemOrList.List, itemOrList.Count);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ItemOrArray<TType> Cast<TType>() => new((TType?) (object?) Item!, (TType[]?) (object?) List, Count);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T[] AsList()
        {
            if (List != null)
                return List;
            if (Count == 0)
                return Default.Array<T>();
            return new[] {Item!};
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T[] ToArray()
        {
            if (List != null)
                return List.ToArray();
            if (Count == 0)
                return Default.Array<T>();
            return new[] {Item!};
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Enumerator GetEnumerator() => new(this);

        #endregion

        #region Nested types

        [StructLayout(LayoutKind.Auto)]
        public struct Enumerator
        {
            #region Fields

            private readonly T _item;
            private readonly T[]? _array;
            private readonly int _count;
            private int _index;

            #endregion

            #region Constructors

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Enumerator(ItemOrArray<T> itemOrList)
            {
                _index = -1;
                _count = itemOrList.Count;
                _item = itemOrList.Item!;
                _array = itemOrList.List;
            }

            #endregion

            #region Properties

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

            #endregion

            #region Implementation of interfaces

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Reset() => _index = -1;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext() => ++_index < _count;

            #endregion
        }

        #endregion
    }
}