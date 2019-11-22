using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using MugenMvvm.Internal;

// ReSharper disable once CheckNamespace
namespace MugenMvvm
{
    public static partial class MugenExtensions
    {
        #region Methods

        public static int Count<TItem, TList>(this ItemOrList<TItem?, TList> itemOrList)
            where TItem : class
            where TList : class?, IReadOnlyCollection<TItem>
        {
            if (itemOrList.List != null)
                return itemOrList.List.Count;
            return itemOrList.Item == null ? 0 : 1;
        }

        public static int Count<TItem>(this ItemOrList<TItem?, List<TItem>> itemOrList)
            where TItem : class
        {
            if (itemOrList.List != null)
                return itemOrList.List.Count;
            return itemOrList.Item == null ? 0 : 1;
        }

        public static int Count<TItem>(this ItemOrList<TItem?, TItem[]> itemOrList)
            where TItem : class
        {
            if (itemOrList.List != null)
                return itemOrList.List.Length;
            return itemOrList.Item == null ? 0 : 1;
        }

        public static void Add<TItem>(this ref ItemOrList<TItem?, List<TItem>> itemOrList, TItem item)
            where TItem : class
        {
            Should.NotBeNull(item, nameof(item));
            if (itemOrList.List != null)
            {
                itemOrList.List.Add(item);
                return;
            }

            if (itemOrList.Item == null)
            {
                itemOrList = item;
                return;
            }

            itemOrList = new ItemOrList<TItem?, List<TItem>>(new List<TItem> { itemOrList.Item, item });
        }

        public static TItem Get<TItem, TList>(this ItemOrList<TItem?, TList> itemOrList, int index)
            where TItem : class
            where TList : class?, IReadOnlyList<TItem>
        {
            if (itemOrList.List != null)
                return itemOrList.List[index];

            if (index == 0 && itemOrList.Item != null)
                return itemOrList.Item;

            ExceptionManager.ThrowIndexOutOfRangeCollection(nameof(index));
            return null;
        }

        public static TItem Get<TItem>(this ItemOrList<TItem?, List<TItem>> itemOrList, int index)
            where TItem : class
        {
            if (itemOrList.List != null)
                return itemOrList.List[index];

            if (index == 0 && itemOrList.Item != null)
                return itemOrList.Item;

            ExceptionManager.ThrowIndexOutOfRangeCollection(nameof(index));
            return null;
        }

        public static TItem Get<TItem>(this ItemOrList<TItem?, TItem[]> itemOrList, int index)
            where TItem : class
        {
            if (itemOrList.List != null)
                return itemOrList.List[index];

            if (index == 0 && itemOrList.Item != null)
                return itemOrList.Item;

            ExceptionManager.ThrowIndexOutOfRangeCollection(nameof(index));
            return null;
        }

        public static void Set<TItem>(this ref ItemOrList<TItem?, List<TItem>> itemOrList, TItem item, int index)
            where TItem : class
        {
            Should.NotBeNull(item, nameof(item));
            if (itemOrList.List != null)
            {
                itemOrList.List[index] = item;
                return;
            }

            if (index == 0 && itemOrList.Item != null)
            {
                itemOrList = item;
                return;
            }

            ExceptionManager.ThrowIndexOutOfRangeCollection(nameof(index));
        }

        public static void Set<TItem, TList>(this ref ItemOrList<TItem?, TList> itemOrList, TItem item, int index)
            where TItem : class
            where TList : class?, IList<TItem>, IReadOnlyCollection<TItem>
        {
            Should.NotBeNull(item, nameof(item));
            if (itemOrList.List != null)
            {
                itemOrList.List[index] = item;
                return;
            }

            if (index == 0 && itemOrList.Item != null)
            {
                itemOrList = item;
                return;
            }

            ExceptionManager.ThrowIndexOutOfRangeCollection(nameof(index));
        }

        public static bool Remove<TItem, TList>(this ref ItemOrList<TItem?, TList> itemOrList, TItem item)
            where TItem : class
            where TList : class?, ICollection<TItem>, IReadOnlyList<TItem>
        {
            if (itemOrList.List != null)
                return itemOrList.List.Remove(item);

            if (Equals(itemOrList.Item, item))
            {
                itemOrList = default;
                return true;
            }

            return false;
        }

        public static void RemoveAt<TItem, TList>(this ref ItemOrList<TItem?, TList> itemOrList, int index)
            where TItem : class
            where TList : class?, IList<TItem>, IReadOnlyList<TItem>
        {
            if (itemOrList.List != null)
            {
                itemOrList.List.RemoveAt(index);
                return;
            }

            if (index == 0 && itemOrList.Item != null)
            {
                itemOrList = default;
                return;
            }

            ExceptionManager.ThrowIndexOutOfRangeCollection(nameof(index));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object? GetRawValue<TItem, TList>(this ItemOrList<TItem?, TList> itemOrList)
            where TItem : class
            where TList : class?, IReadOnlyCollection<TItem>
        {
            return (object?)itemOrList.Item ?? itemOrList.List;
        }

        public static void Merge<TItem, TList>(this ItemOrList<TItem?, TList> itemOrList, ref TItem? currentItem, ref List<TItem>? items)
            where TItem : class
            where TList : class?, IReadOnlyCollection<TItem>
        {
            var list = itemOrList.List;
            var item = itemOrList.Item;
            if (item == null && list == null)
                return;

            if (list == null)
            {
                if (currentItem == null)
                    currentItem = item;
                else
                {
                    if (items == null)
                        items = new List<TItem>();
                    if (items.Count == 0)
                        items.Add(currentItem);
                    items.Add(item!);
                }
            }
            else
            {
                if (items == null)
                    items = new List<TItem>();
                if (currentItem != null && items.Count == 0)
                    items.Add(currentItem);
                items.AddRange(list);
            }
        }

        public static bool IsNullOrEmpty<TItem, TList>(this ItemOrList<TItem, TList> itemOrList)
            where TItem : class?
            where TList : class?, IReadOnlyCollection<TItem>
        {
            return itemOrList.Item == null && itemOrList.List == null;
        }

        public static TResult[] ToArray<T, TResult>(this IReadOnlyCollection<T> collection, Func<T, TResult> selector)
        {
            Should.NotBeNull(collection, nameof(collection));
            var count = collection.Count;
            if (count == 0)
                return Default.EmptyArray<TResult>();
            var array = new TResult[count];
            count = 0;
            foreach (var item in collection)
                array[count++] = selector(item);
            return array;
        }

        public static void AddComponentOrdered<T>(ref T[] items, T item, object owner) where T : class
        {
            var componentComparer = ComponentComparer<T>.GetComparer(owner);
            try
            {
                AddOrdered(ref items, item, componentComparer);
            }
            finally
            {
                componentComparer.Release();
            }
        }

        public static void AddOrdered<T>(ref T[] items, T item, IComparer<T> comparer)
        {
            var array = new T[items.Length + 1];
            Array.Copy(items, 0, array, 0, items.Length);
            AddOrdered(array, item, items.Length, comparer);
            items = array;
        }

        public static void AddOrdered<T>(T[] items, T item, int size, IComparer<T> comparer)
        {
            var binarySearch = Array.BinarySearch(items, 0, size, item, comparer);
            if (binarySearch < 0)
                binarySearch = ~binarySearch;
            if (binarySearch < size)
                Array.Copy(items, binarySearch, items, binarySearch + 1, size - binarySearch);
            items[binarySearch] = item;
        }

        public static void AddOrdered<T>(List<T> list, T item, IComparer<T> comparer)
        {
            Should.NotBeNull(list, nameof(list));
            var binarySearch = list.BinarySearch(item, comparer);
            if (binarySearch < 0)
                binarySearch = ~binarySearch;
            list.Insert(binarySearch, item);
        }

        public static bool Remove<T>(ref T[] items, T item) where T : class
        {
            if (items.Length == 0)
                return false;
            if (items.Length == 1)
            {
                if (ReferenceEquals(items[0], items))
                {
                    items = Default.EmptyArray<T>();
                    return true;
                }

                return false;
            }

            T[]? array = null;
            for (var i = 0; i < items.Length; i++)
            {
                if (array == null && ReferenceEquals(item, items[i]))
                {
                    array = new T[items.Length - 1];
                    Array.Copy(items, 0, array, 0, i);
                    continue;
                }

                if (array != null)
                    array[i - 1] = items[i];
            }

            if (array != null)
                items = array;
            return array != null;
        }

        #endregion

        #region Nested types

        private sealed class ComponentComparer<T> : IComparer<T> where T : class
        {
            #region Fields

            private object? _owner;
            private readonly bool _isPool;
            private static readonly ComponentComparer<T> Instance = new ComponentComparer<T>(true);
            private static ComponentComparer<T>? _poolComparer = Instance;

            #endregion

            #region Constructors

            private ComponentComparer(bool isPool = false)
            {
                _isPool = isPool;
            }

            #endregion

            #region Implementation of interfaces

            int IComparer<T>.Compare(T x, T y)
            {
                return GetComponentPriority(y, _owner).CompareTo(GetComponentPriority(x, _owner));
            }

            #endregion

            #region Methods

            public static ComponentComparer<T> GetComparer(object? owner)
            {
                var comparer = Interlocked.Exchange(ref _poolComparer, null) ?? new ComponentComparer<T>();
                comparer._owner = owner;
                return comparer;
            }

            public void Release()
            {
                if (!_isPool)
                    return;
                _owner = null;
                _poolComparer = this;
            }

            #endregion
        }

        #endregion
    }
}