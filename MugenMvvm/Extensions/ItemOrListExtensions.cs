using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using MugenMvvm.Collections;
using MugenMvvm.Internal;

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

        [return: MaybeNull]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T FirstOrDefault<T>(this ItemOrIEnumerable<T> itemOrList)
        {
            if (itemOrList.List != null)
            {
                foreach (var item in itemOrList)
                    return item;
                return default;
            }

            return itemOrList.Item;
        }

        [return: MaybeNull]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T FirstOrDefault<T>(this ItemOrArray<T> itemOrList)
        {
            if (itemOrList.List != null)
            {
                foreach (var item in itemOrList)
                    return item;
                return default;
            }

            return itemOrList.Item;
        }

        [return: MaybeNull]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T FirstOrDefault<T>(this ItemOrIReadOnlyList<T> itemOrList)
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

        #endregion
    }
}