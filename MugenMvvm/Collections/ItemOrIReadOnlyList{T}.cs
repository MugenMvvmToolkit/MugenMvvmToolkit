using System;
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
        internal readonly int FixedCount;
        public readonly T? Item;
        public readonly IReadOnlyList<T>? List;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ItemOrIReadOnlyList(T? item)
        {
            Item = item;
            List = null;
            FixedCount = 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ItemOrIReadOnlyList(T? item, IReadOnlyList<T>? list, int fixedCount)
        {
            Item = item!;
            List = list;
            FixedCount = fixedCount;
        }

        public bool IsEmpty
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => FixedCount == 0 && List == null;
        }
        
        public bool HasItem
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => FixedCount == 1 && List == null;
        }
        
        public int Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (FixedCount != 0)
                    return FixedCount;
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
                    if (FixedCount != 0)
                        return ((T[])List)[index];
                    return List[index];
                }

                if ((uint)index < (uint)FixedCount)
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
        public static implicit operator ItemOrIReadOnlyList<T>(List<T>? items) => ItemOrIReadOnlyList.FromList(items);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ItemOrIEnumerable<T>(ItemOrIReadOnlyList<T> itemOrList) => new(itemOrList.Item!, itemOrList.List, itemOrList.FixedCount);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ItemOrIReadOnlyCollection<T>(ItemOrIReadOnlyList<T> itemOrList) => new(itemOrList.Item!, itemOrList.List, itemOrList.FixedCount);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<T> AsEnumerable()
        {
            if (List != null)
                return List;
            if (FixedCount == 0)
                return Default.Enumerable<T>();
            return Default.SingleItemEnumerable(Item!);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IReadOnlyList<T> AsList()
        {
            if (List != null)
                return List;
            if (FixedCount == 0)
                return Default.ReadOnlyList<T>();
            return new[] { Item! };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public List<T> ToList()
        {
            if (List != null)
                return new List<T>(List);
            if (FixedCount == 0)
                return new List<T>();
            return new List<T> { Item! };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T[] ToArray()
        {
            if (List != null)
                return List.ToArray();
            if (FixedCount == 0)
                return Array.Empty<T>();
            return new[] { Item! };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ItemOrListEnumerator<T> GetEnumerator() => new(this);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private IEnumerator<T> GetEnumeratorRef()
        {
            if (List != null)
                return List.GetEnumerator();
            if (FixedCount == 0)
                return Default.Enumerator<T>();
            return Default.SingleItemEnumerator(Item!);
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumeratorRef();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumeratorRef();
    }
}