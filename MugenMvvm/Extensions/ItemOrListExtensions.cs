using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using MugenMvvm.Collections;

namespace MugenMvvm.Extensions
{
    public static partial class MugenExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object? GetRawValue<T>(this ItemOrArray<T> itemOrList)
            where T : class? => itemOrList.Item ?? (object?) itemOrList.List;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object? GetRawValue<T>(this ItemOrIEnumerable<T> itemOrList)
            where T : class? => itemOrList.Item ?? (object?) itemOrList.List;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object? GetRawValue<T>(this ItemOrIReadOnlyCollection<T> itemOrList)
            where T : class? => itemOrList.Item ?? (object?) itemOrList.List;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object? GetRawValue<T>(this ItemOrIReadOnlyList<T> itemOrList)
            where T : class? => itemOrList.Item ?? (object?) itemOrList.List;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object? GetRawValue<T>(this ItemOrListEditor<T> editor)
            where T : class? => editor.GetRawValueInternal();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T? FirstOrDefault<T>(this ItemOrIEnumerable<T> itemOrList)
        {
            foreach (var item in itemOrList)
                return item;
            return default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T? FirstOrDefault<T>(this ItemOrArray<T> itemOrList)
        {
            foreach (var item in itemOrList)
                return item;
            return default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T? FirstOrDefault<T>(this ItemOrIReadOnlyCollection<T> itemOrList)
        {
            foreach (var item in itemOrList)
                return item;
            return default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T? FirstOrDefault<T>(this ItemOrIReadOnlyList<T> itemOrList)
        {
            foreach (var item in itemOrList)
                return item;
            return default;
        }

        public static bool Contains<T>(this ItemOrIEnumerable<T> itemOrList, T value, IEqualityComparer<T>? comparer = null)
        {
            if (comparer == null && itemOrList.List is ICollection<T> collection)
                return collection.Contains(value);
            foreach (var v in itemOrList)
            {
                if (comparer.EqualsOrDefault(v, value))
                    return true;
            }

            return false;
        }

        public static bool Contains<T>(this ItemOrIReadOnlyCollection<T> itemOrList, T value, IEqualityComparer<T>? comparer = null)
        {
            if (comparer == null && itemOrList.List is ICollection<T> collection)
                return collection.Contains(value);
            foreach (var v in itemOrList)
            {
                if (comparer.EqualsOrDefault(v, value))
                    return true;
            }

            return false;
        }

        public static bool Contains<T>(this ItemOrIReadOnlyList<T> itemOrList, T value, IEqualityComparer<T>? comparer = null)
        {
            if (comparer == null && itemOrList.List is ICollection<T> collection)
                return collection.Contains(value);
            foreach (var v in itemOrList)
            {
                if (comparer.EqualsOrDefault(v, value))
                    return true;
            }

            return false;
        }

        public static bool Contains<T>(this ItemOrArray<T> itemOrList, T value, IEqualityComparer<T>? comparer = null)
        {
            if (comparer == null && itemOrList.List != null)
                return Array.IndexOf(itemOrList.List, value) >= 0;
            foreach (var v in itemOrList)
            {
                if (comparer.EqualsOrDefault(v, value))
                    return true;
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddIfNotNull<T>(this ref ItemOrListEditor<T> editor, T? item) where T : class
        {
            if (item != null)
                editor.Add(item);
        }

        public static void SetAt<T>(this ref ItemOrArray<T> array, int index, T value)
        {
            if (array.List != null)
                array.List[index] = value;
            else if ((uint) index < (uint) array.Count)
                array = new ItemOrArray<T>(value);
            else
                ExceptionManager.ThrowIndexOutOfRangeCollection(nameof(index));
        }

        public static ItemOrArray<T> ToItemOrArray<T>(this List<T> list)
        {
            if (list.Count == 0)
                return default;
            if (list.Count == 1)
                return new ItemOrArray<T>(list[0]);
            return list.ToArray();
        }

        public static ItemOrArray<T> ToItemOrArray<T>(this IList<T> list)
        {
            if (list.Count == 0)
                return default;
            if (list.Count == 1)
                return new ItemOrArray<T>(list[0]);
            return list.ToArray();
        }

        public static ItemOrArray<T> ToItemOrArray<T>(this ICollection<T> list)
        {
            if (list.Count == 0)
                return default;
            if (list.Count == 1)
                return new ItemOrArray<T>(list.ElementAt(0));
            return list.ToArray();
        }

        internal static ItemOrIReadOnlyList<T> ToItemOrList<T>(this List<T> list, bool clear)
        {
            if (list.Count == 0)
                return default;
            if (list.Count == 1)
            {
                var r = list[0];
                if (clear)
                    list.Clear();
                return r;
            }

            var array = list.ToArray();
            if (clear)
                list.Clear();
            return array;
        }

        internal static TValue[] ToArray<T, TValue>(this ItemOrIReadOnlyList<T> itemOrList, Func<T, TValue> selector)
        {
            if (itemOrList.Count == 0)
                return Array.Empty<TValue>();
            var values = new TValue[itemOrList.Count];
            for (var i = 0; i < values.Length; i++)
                values[i] = selector(itemOrList[i]);
            return values;
        }

        internal static int Count<T>(this ItemOrIReadOnlyList<T> itemOrList, Func<T, bool> predicate)
        {
            var count = 0;
            foreach (var item in itemOrList)
            {
                if (predicate(item))
                    ++count;
            }

            return count;
        }

        internal static bool All<T>(this ItemOrArray<T> itemOrList, Func<T, bool> predicate)
        {
            foreach (var item in itemOrList)
            {
                if (!predicate(item))
                    return false;
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static T Last<T>(this ItemOrIReadOnlyList<T> itemOrList) => itemOrList[itemOrList.Count - 1];

        internal static T? FirstOrDefault<T, TState>(this ItemOrArray<T> itemOrList, TState state, Func<T, TState, bool> predicate)
        {
            foreach (var item in itemOrList)
            {
                if (predicate(item, state))
                    return item;
            }

            return default;
        }
    }
}