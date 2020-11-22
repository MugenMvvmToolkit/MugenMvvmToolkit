using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using MugenMvvm.Internal;

namespace MugenMvvm.Extensions
{
    public static partial class MugenExtensions
    {
        #region Methods

        [return: MaybeNull]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TItem FirstOrDefault<TItem, TList>(this ItemOrList<TItem, TList> itemOrList)
            where TList : class, IEnumerable<TItem>
        {
            if (itemOrList.List != null)
            {
                foreach (var item in itemOrList)
                    return item;
                return default;
            }

            return itemOrList.Item;
        }

        public static TItem[] ToArray<TItem, TList>(this ItemOrList<TItem, TList> itemOrList)
            where TList : class, IEnumerable<TItem>
        {
            if (itemOrList.IsEmpty)
                return Default.Array<TItem>();
            var items = new TItem[itemOrList.Count];
            var index = 0;
            foreach (var item in itemOrList)
                items[index++] = item;
            return items;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<TItem> AsEnumerable<TItem, TList>(this ItemOrList<TItem, TList> itemOrList)
            where TList : class, IEnumerable<TItem>
        {
            if (itemOrList.List != null)
                return itemOrList.List;
            if (itemOrList.HasItem)
                return new[] {itemOrList.Item}!;
            return Enumerable.Empty<TItem>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IList<TItem> AsList<TItem>(this ItemOrList<TItem, IList<TItem>> itemOrList)
            => itemOrList.AsList(ListClosure<TItem>.DefaultList, ListClosure<TItem>.ItemList);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TItem[] AsList<TItem>(this ItemOrList<TItem, TItem[]> itemOrList)
            => itemOrList.AsList(ListClosure<TItem>.DefaultList, ListClosure<TItem>.ItemList);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IReadOnlyList<TItem> AsList<TItem>(this ItemOrList<TItem, IReadOnlyList<TItem>> itemOrList)
            => itemOrList.AsList(ListClosure<TItem>.DefaultList, ListClosure<TItem>.ItemList);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IReadOnlyCollection<TItem> AsList<TItem>(this ItemOrList<TItem, IReadOnlyCollection<TItem>> itemOrList)
            => itemOrList.AsList(ListClosure<TItem>.DefaultList, ListClosure<TItem>.ItemList);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TList AsList<TItem, TList>(this ItemOrList<TItem, TList> itemOrList, Func<TList> getDefaultList, Func<TItem, TList> getItemList)
            where TList : class, IEnumerable<TItem>
        {
            Should.NotBeNull(getDefaultList, nameof(getItemList));
            Should.NotBeNull(getItemList, nameof(getItemList));
            if (itemOrList.List != null)
                return itemOrList.List;
            if (itemOrList.HasItem)
                return getItemList(itemOrList.Item!);
            return getDefaultList();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object? GetRawValue<TItem, TList>(this ItemOrList<TItem, TList> itemOrList)
            where TItem : class?
            where TList : class, IEnumerable<TItem> =>
            (object?) itemOrList.Item ?? itemOrList.List;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object? GetRawValue<TItem, TList>(this ItemOrListEditor<TItem, TList> itemOrList)
            where TItem : class?
            where TList : class, IList<TItem> =>
            itemOrList.GetRawValueInternal();

        public static ItemOrListEditor<TItem, TList> AddIfNotNull<TItem, TList>(this ref ItemOrListEditor<TItem, TList> editor, [AllowNull] TItem value)
            where TItem : class?
            where TList : class, IList<TItem>
        {
            if (value != null)
                return editor.Add(value);
            return editor;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ItemOrListEditor<TItem, IList<TItem>> Editor<TItem>(this ItemOrList<TItem, IList<TItem>> itemOrList) =>
            new ItemOrListEditor<TItem, IList<TItem>>(itemOrList.Item, itemOrList.List, itemOrList.HasItem, () => new List<TItem>());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ItemOrListEditor<TItem, List<TItem>> Editor<TItem>(this ItemOrList<TItem, List<TItem>> itemOrList) =>
            new ItemOrListEditor<TItem, List<TItem>>(itemOrList.Item, itemOrList.List, itemOrList.HasItem, () => new List<TItem>());

        #endregion

        #region Nested types

        private static class ListClosure<T>
        {
            #region Fields

            public static readonly Func<T[]> DefaultList = Default.Array<T>;
            public static readonly Func<T, T[]> ItemList = arg => new[] {arg};

            #endregion
        }

        #endregion
    }
}