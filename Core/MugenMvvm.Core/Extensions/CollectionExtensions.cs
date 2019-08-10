using System;
using System.Collections.Generic;

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

        public static void AddOrdered<T>(ref T[] items, T component, object owner) where T : class
        {
            var array = new T[items.Length + 1];
            var added = false;
            var priority = GetComponentPriority(component, owner);
            for (var i = 0; i < items.Length; i++)
            {
                if (added)
                {
                    array[i + 1] = items[i];
                    continue;
                }

                var oldItem = items[i];
                var compareTo = priority.CompareTo(GetComponentPriority(oldItem, owner));
                if (compareTo > 0)
                {
                    array[i] = component;
                    added = true;
                    --i;
                }
                else
                    array[i] = oldItem;
            }

            if (!added)
                array[array.Length - 1] = component;
            items = array;
        }

        public static bool Remove<T>(ref T[] items, T item) where T : class
        {
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