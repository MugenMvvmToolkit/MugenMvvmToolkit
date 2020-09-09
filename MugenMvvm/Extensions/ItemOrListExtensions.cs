using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using MugenMvvm.Internal;

namespace MugenMvvm.Extensions
{
    public static partial class MugenExtensions
    {
        #region Methods

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object? GetRawValue<TItem, TList>(this ItemOrList<TItem, TList> itemOrList)
            where TItem : class?
            where TList : class, IEnumerable<TItem> =>
            (object?)itemOrList.Item ?? itemOrList.List;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object? GetRawValue<TItem, TList>(this ItemOrListEditor<TItem, TList> itemOrList)
            where TItem : class?
            where TList : class, IList<TItem> =>
            itemOrList.GetRawValueInternal();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNullOrEmpty<TItem, TList>(this ItemOrList<TItem, TList> itemOrList)
            where TItem : class?
            where TList : class, IEnumerable<TItem> =>
            itemOrList.Item == null && itemOrList.List == null;

        public static Task WhenAll<TList>(this ItemOrList<Task, TList> itemOrList) where TList : class, IEnumerable<Task>
        {
            if (itemOrList.Item != null)
                return itemOrList.Item;
            if (itemOrList.List != null)
                return Task.WhenAll(itemOrList.List);
            return Default.CompletedTask;
        }

        [return: MaybeNull]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TItem FirstOrDefault<TItem, TList>(this ItemOrList<TItem, TList> itemOrList)
            where TList : class, IEnumerable<TItem>
        {
            if (itemOrList.List != null)
                return Enumerable.FirstOrDefault(itemOrList.List);
            return itemOrList.Item;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlyListIterator<TItem, TList> Iterator<TItem, TList>(this ItemOrList<TItem, TList> itemOrList)
            where TItem : class
            where TList : class, IReadOnlyList<TItem>
        {
            if (itemOrList.List != null)
                return new ReadOnlyListIterator<TItem, TList>(itemOrList.List.Count, null, itemOrList.List);
            return itemOrList.Item == null ? default : new ReadOnlyListIterator<TItem, TList>(1, itemOrList.Item, null);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlyListIterator<TItem, TList> Iterator<TItem, TList>(this ItemOrList<TItem, TList> itemOrList, Func<TItem, bool> isEmpty)
            where TList : class, IReadOnlyList<TItem>
        {
            Should.NotBeNull(isEmpty, nameof(isEmpty));
            if (itemOrList.List != null)
                return new ReadOnlyListIterator<TItem, TList>(itemOrList.List.Count, default, itemOrList.List);
            return isEmpty(itemOrList.Item!) ? default : new ReadOnlyListIterator<TItem, TList>(1, itemOrList.Item, null);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ListIterator<TItem, IList<TItem>> Iterator<TItem>(this ItemOrList<TItem, IList<TItem>> itemOrList)
            where TItem : class
        {
            if (itemOrList.List != null)
                return new ListIterator<TItem, IList<TItem>>(itemOrList.List.Count, null, itemOrList.List);
            return itemOrList.Item == null ? default : new ListIterator<TItem, IList<TItem>>(1, itemOrList.Item, null);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ListIterator<TItem, IList<TItem>> Iterator<TItem>(this ItemOrList<TItem, IList<TItem>> itemOrList, Func<TItem, bool> isEmpty)
        {
            Should.NotBeNull(isEmpty, nameof(isEmpty));
            if (itemOrList.List != null)
                return new ListIterator<TItem, IList<TItem>>(itemOrList.List.Count, default, itemOrList.List);
            return isEmpty(itemOrList.Item!) ? default : new ListIterator<TItem, IList<TItem>>(1, itemOrList.Item, null);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ListIterator<TItem, List<TItem>> Iterator<TItem>(this ItemOrList<TItem, List<TItem>> itemOrList)
            where TItem : class
        {
            if (itemOrList.List != null)
                return new ListIterator<TItem, List<TItem>>(itemOrList.List.Count, null, itemOrList.List);
            return itemOrList.Item == null ? default : new ListIterator<TItem, List<TItem>>(1, itemOrList.Item, null);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ListIterator<TItem, List<TItem>> Iterator<TItem>(this ItemOrList<TItem, List<TItem>> itemOrList, Func<TItem, bool> isEmpty)
        {
            Should.NotBeNull(isEmpty, nameof(isEmpty));
            if (itemOrList.List != null)
                return new ListIterator<TItem, List<TItem>>(itemOrList.List.Count, default, itemOrList.List);
            return isEmpty(itemOrList.Item!) ? default : new ListIterator<TItem, List<TItem>>(1, itemOrList.Item, null);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ArrayIterator<TItem> Iterator<TItem>(this ItemOrList<TItem, TItem[]> itemOrList)
            where TItem : class
        {
            if (itemOrList.List != null)
                return new ArrayIterator<TItem>(itemOrList.List.Length, null, itemOrList.List);
            return itemOrList.Item == null ? default : new ArrayIterator<TItem>(1, itemOrList.Item, null);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ArrayIterator<TItem> Iterator<TItem>(this ItemOrList<TItem, TItem[]> itemOrList, Func<TItem, bool> isEmpty)
        {
            Should.NotBeNull(isEmpty, nameof(isEmpty));
            if (itemOrList.List != null)
                return new ArrayIterator<TItem>(itemOrList.List.Length, default, itemOrList.List);
            return isEmpty(itemOrList.Item!) ? default : new ArrayIterator<TItem>(1, itemOrList.Item, null);
        }

        public static ItemOrListEditor<TItem, IList<TItem>> Editor<TItem>(this ItemOrList<TItem, IList<TItem>> itemOrList)
            where TItem : class =>
            itemOrList.Editor(item => item == null);

        public static ItemOrListEditor<TItem, IList<TItem>> Editor<TItem>(this ItemOrList<TItem, IList<TItem>> itemOrList, Func<TItem, bool> isEmpty) =>
            new ItemOrListEditor<TItem, IList<TItem>>(itemOrList.Item, itemOrList.List, isEmpty, () => new List<TItem>());

        public static ItemOrListEditor<TItem, List<TItem>> Editor<TItem>(this ItemOrList<TItem, List<TItem>> itemOrList)
            where TItem : class =>
            itemOrList.Editor(item => item == null);

        public static ItemOrListEditor<TItem, List<TItem>> Editor<TItem>(this ItemOrList<TItem, List<TItem>> itemOrList, Func<TItem, bool> isEmpty) =>
            new ItemOrListEditor<TItem, List<TItem>>(itemOrList.Item, itemOrList.List, isEmpty, () => new List<TItem>());

        #endregion
    }
}