using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MugenMvvm.Collections
{
    [StructLayout(LayoutKind.Auto)]
    internal struct ListInternal<T>
    {
        private const int DefaultCapacity = 4;

        public int Count;
        public T[] Items;

        public ListInternal(int capacity)
        {
            Items = capacity == 0 ? Array.Empty<T>() : new T[capacity];
            Count = 0;
        }

        [MemberNotNullWhen(false, nameof(Items))]
        public bool IsEmpty => Items == null;

        public int Capacity
        {
            readonly get => Items.Length;
            set
            {
                if (value != Items.Length)
                {
                    if (value > 0)
                    {
                        T[] newItems = new T[value];
                        if (Count > 0) Array.Copy(Items, 0, newItems, 0, Count);

                        Items = newItems;
                    }
                    else
                        Items = Array.Empty<T>();
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(T item)
        {
            T[] array = Items;
            var size = Count;
            if ((uint)size < (uint)array.Length)
            {
                Count = size + 1;
                array[size] = item;
            }
            else
                AddWithResize(item);
        }

        public int AddOrdered(T item, IComparer<T>? comparer = null)
        {
            var binarySearch = BinarySearch(item, comparer);
            if (binarySearch < 0)
                binarySearch = ~binarySearch;
            Insert(binarySearch, item);
            return binarySearch;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int BinarySearch(T item, IComparer<T>? comparer = null) => BinarySearch(0, Count, item, comparer);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int BinarySearch(int index, int count, T item, IComparer<T>? comparer) => Array.BinarySearch(Items, index, count, item, comparer);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
#if NET461
            var size = Count;
            Count = 0;
            if (size > 0)
                Array.Clear(Items, 0, size);
#else
            if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            {
                var size = Count;
                Count = 0;
                if (size > 0)
                    Array.Clear(Items, 0, size);
            }
            else
                Count = 0;
#endif
        }

        public readonly bool Contains(T item) => Count != 0 && IndexOf(item) != -1;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int IndexOf(T item) => Count == 0 ? -1 : Array.IndexOf(Items, item, 0, Count);

        public void Insert(int index, T item)
        {
            if (Count == Items.Length)
                EnsureCapacity(Count + 1);
            if (index < Count) Array.Copy(Items, index, Items, index + 1, Count - index);

            Items[index] = item;
            Count++;
        }

        public bool Remove(T item)
        {
            var index = IndexOf(item);
            if (index >= 0)
            {
                RemoveAt(index);
                return true;
            }

            return false;
        }

        public void RemoveAt(int index)
        {
            Count--;
            if (index < Count)
                Array.Copy(Items, index + 1, Items, index, Count - index);

#if !NET461
            if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
#endif
                Items[Count] = default!;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T RemoveAtResult(int index)
        {
            var foo = Items[index];
            RemoveAt(index);
            return foo;
        }

        public readonly void Sort(IComparer<T>? comparer = null)
            => Sort(0, Count, comparer);

        public readonly void Sort(int index, int count, IComparer<T>? comparer)
        {
            if (count > 1)
                Array.Sort(Items, index, count, comparer);
        }

        public readonly T[] ToArray()
        {
            if (Count == 0)
                return Array.Empty<T>();

            T[] array = new T[Count];
            Array.Copy(Items, 0, array, 0, Count);
            return array;
        }

        public void EnsureCapacity(int min)
        {
            if (Items.Length < min)
            {
                var newCapacity = Items.Length == 0 ? DefaultCapacity : Items.Length * 2;
                if ((uint)newCapacity > 2146435071U)
                    newCapacity = 2146435071;
                if (newCapacity < min)
                    newCapacity = min;
                Capacity = newCapacity;
            }
        }

        private void AddWithResize(T item)
        {
            var size = Count;
            EnsureCapacity(size + 1);
            Count = size + 1;
            Items[size] = item;
        }
    }
}