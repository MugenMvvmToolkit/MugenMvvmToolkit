using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace MugenMvvm.Collections
{
    internal class ListInternal<T>
    {
        private const int DefaultCapacity = 4;

        public T[] Items;

        public ListInternal(int capacity)
        {
            Items = capacity == 0 ? Array.Empty<T>() : new T[capacity];
        }

        public int Capacity
        {
            get => Items.Length;
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

        public int Count { get; private set; }

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

        public int AddOrdered(T item, IComparer<T> comparer)
        {
            var binarySearch = BinarySearch(item, comparer);
            if (binarySearch < 0)
                binarySearch = ~binarySearch;
            Insert(binarySearch, item);
            return binarySearch;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int BinarySearch(T item, IComparer<T>? comparer)
            => BinarySearch(0, Count, item, comparer);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int BinarySearch(int index, int count, T item, IComparer<T>? comparer) => Array.BinarySearch(Items, index, count, item, comparer);

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

        public bool Contains(T item) => Count != 0 && IndexOf(item) != -1;

        public int IndexOf(T item)
            => Array.IndexOf(Items, item, 0, Count);

        public int IndexOf(T item, int index) => Array.IndexOf(Items, item, index, Count - index);

        public int IndexOf(T item, int index, int count) => Array.IndexOf(Items, item, index, count);

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

        public void Sort(IComparer<T>? comparer)
            => Sort(0, Count, comparer);

#if NET5_0
        public void Sort(Comparison<T> comparison)
            => Items.AsSpan(0, Count).Sort(comparison);
#endif

        public void Sort(int index, int count, IComparer<T>? comparer)
        {
            if (count > 1)
                Array.Sort(Items, index, count, comparer);
        }

        public T[] ToArray()
        {
            if (Count == 0)
                return Array.Empty<T>();

            T[] array = new T[Count];
            Array.Copy(Items, 0, array, 0, Count);
            return array;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void AddWithResize(T item)
        {
            var size = Count;
            EnsureCapacity(size + 1);
            Count = size + 1;
            Items[size] = item;
        }

        private void EnsureCapacity(int min)
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
    }
}