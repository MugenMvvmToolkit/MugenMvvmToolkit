using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using MugenMvvm.Collections;

namespace MugenMvvm.Extensions
{
    public static partial class MugenExtensions
    {
        #region Methods

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object? GetRawValue<T>(this ItemOrArray<T> itemOrList)
            where T : class? => itemOrList.Item ?? (object?) itemOrList.List;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object? GetRawValue<T>(this ItemOrIEnumerable<T> itemOrList)
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
            if (itemOrList.List != null)
            {
                foreach (var item in itemOrList)
                    return item;
                return default;
            }

            return itemOrList.Item;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T? FirstOrDefault<T>(this ItemOrArray<T> itemOrList)
        {
            if (itemOrList.List != null)
            {
                foreach (var item in itemOrList)
                    return item;
                return default;
            }

            return itemOrList.Item;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T? FirstOrDefault<T>(this ItemOrIReadOnlyList<T> itemOrList)
        {
            if (itemOrList.List != null)
            {
                foreach (var item in itemOrList)
                    return item;
                return default;
            }

            return itemOrList.Item;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddIfNotNull<T>(this ref ItemOrListEditor<T> editor, T? item) where T : class
        {
            if (item != null)
                editor.Add(item);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetAt<T>(this ref ItemOrArray<T> array, int index, T value)
        {
            if (array.List != null)
                array.List[index] = value;
            else if ((uint) index < (uint) array.Count)
                array = new ItemOrArray<T>(value, true);
            else
                ExceptionManager.ThrowIndexOutOfRangeCollection(nameof(index));
        }

        internal static ItemOrIReadOnlyList<T> ToItemOrList<T>(this List<T> list, bool clear)
        {
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

        internal static int Count<T>(this ItemOrIReadOnlyList<T> itemOrList, Func<T, bool> predicate)
        {
            int count = 0;
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

        internal static T? FirstOrDefault<T>(this ItemOrArray<T> itemOrList, Func<T, bool> predicate)
        {
            if (itemOrList.List != null)
            {
                foreach (var item in itemOrList)
                {
                    if (predicate(item))
                        return item;
                }
                return default;
            }

            if (predicate(itemOrList.Item!))
                return itemOrList.Item;
            return default;
        }

        #endregion
    }
}