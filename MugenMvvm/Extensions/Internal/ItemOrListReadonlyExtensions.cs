using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MugenMvvm.Internal;

namespace MugenMvvm.Extensions.Internal
{
    public static class ItemOrListReadonlyExtensions
    {
        #region Methods

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Count<TItem, TList>(this ItemOrList<TItem, TList> itemOrList)
            where TItem : class?
            where TList : class, IReadOnlyCollection<TItem>
        {
            if (itemOrList.List != null)
                return itemOrList.List.Count;
            return itemOrList.Item == null ? 0 : 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Count<TItem, TList>(this ItemOrList<TItem, TList> itemOrList, Func<TItem, bool> isNullOrEmpty)
            where TList : class, IReadOnlyCollection<TItem>
        {
            if (itemOrList.List != null)
                return itemOrList.List.Count;
            return isNullOrEmpty(itemOrList.Item!) ? 0 : 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TItem Get<TItem, TList>(this ItemOrList<TItem, TList> itemOrList, int index)
            where TList : class, IReadOnlyList<TItem>
        {
            if (itemOrList.List != null)
                return itemOrList.List[index];

            if (index == 0)
                return itemOrList.Item!;

            ExceptionManager.ThrowIndexOutOfRangeCollection(nameof(index));
            return default;
        }

        #endregion
    }
}