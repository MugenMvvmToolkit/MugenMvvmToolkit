using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Collections.Components;
using MugenMvvm.Internal;

namespace MugenMvvm.Extensions
{
    public static partial class MugenExtensions
    {
        #region Methods

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Add<T>(this ref LazyList<T> list, T item)
        {
            list.Get().Add(item);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddIfNotNull<T>(this ref LazyList<T> list, T? item) where T : class
        {
            if (item != null)
                list.Get().Add(item);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddRange<T>(this ref LazyList<T> list, IReadOnlyCollection<T>? items)
        {
            if (items != null && items.Count != 0)
                list.Get().AddRange(items);
        }

        public static ActionToken TryLock<T>(this IObservableCollection<T>? observableCollection)
        {
            if (!(observableCollection is ICollection c))
                return default;

            bool taken = false;
            var locker = c.SyncRoot;
            Monitor.Enter(locker, ref taken);
            if (taken)
                return new ActionToken((o, _) => Monitor.Exit(o), locker);
            return default;
        }

        [return: NotNullIfNotNull("observableCollection")]
        public static IEnumerable<T>? DecorateItems<T>(this IObservableCollection<T>? observableCollection)
        {
            if (observableCollection == null)
                return null;
            var component = observableCollection.GetComponentOptional<IDecoratorManagerObservableCollectionComponent<T>>();
            if (component == null)
                return observableCollection;
            return component.DecorateItems();
        }

        public static void AddRange<T>(this ICollection<T> items, IReadOnlyList<T> value)
        {
            Should.NotBeNull(items, nameof(items));
            Should.NotBeNull(value, nameof(value));
            for (int i = 0; i < value.Count; i++)
                items.Add(value[i]);
        }

        public static void AddRange<T>(this ICollection<T> items, IList<T> value)
        {
            Should.NotBeNull(items, nameof(items));
            Should.NotBeNull(value, nameof(value));
            for (int i = 0; i < value.Count; i++)
                items.Add(value[i]);
        }

        public static T[] ToArray<T>(this T[] array)
        {
            Should.NotBeNull(array, nameof(array));
            var result = new T[array.Length];
            Array.Copy(array, 0, result, 0, array.Length);
            return result;
        }

        public static TResult[] ToArray<T, TResult>(this IReadOnlyCollection<T> collection, Func<T, TResult> selector)
        {
            Should.NotBeNull(collection, nameof(collection));
            var count = collection.Count;
            if (count == 0)
                return Default.Array<TResult>();
            var array = new TResult[count];
            count = 0;
            foreach (var item in collection)
                array[count++] = selector(item);
            return array;
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
                if (ReferenceEquals(items[0], item))
                {
                    items = Default.Array<T>();
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
    }
}