using System;
using System.Collections.Generic;
using System.Threading;

// ReSharper disable once CheckNamespace
namespace MugenMvvm
{
    public static partial class MugenExtensions
    {
        #region Methods

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

        public static void AddOrdered<T>(List<T> list, T item, IComparer<T> comparer)
        {
            Should.NotBeNull(list, nameof(list));
            var binarySearch = list.BinarySearch(item, comparer);
            if (binarySearch < 0)
                binarySearch = ~binarySearch;
            list.Insert(binarySearch, item);
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