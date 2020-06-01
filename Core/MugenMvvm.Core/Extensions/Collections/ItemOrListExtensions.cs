using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Internal;

namespace MugenMvvm.Extensions.Collections
{
    public static class ItemOrListExtensions
    {
        #region Methods

        public static int Count<TItem, TList>(this ItemOrList<TItem, TList> itemOrList)
            where TItem : class?
            where TList : class, IReadOnlyCollection<TItem>
        {
            if (itemOrList.Item != null)
                return 1;
            if (itemOrList.List != null)
                return itemOrList.List.Count;
            return itemOrList.Item == null ? 0 : 1;
        }

        public static TItem Get<TItem, TList>(this ItemOrList<TItem, TList> itemOrList, int index)
            where TItem : class?
            where TList : class, IReadOnlyList<TItem>
        {
            if (itemOrList.List != null)
                return itemOrList.List[index];

            if (index == 0 && itemOrList.Item != null)
                return itemOrList.Item;

            ExceptionManager.ThrowIndexOutOfRangeCollection(nameof(index));
            return null;
        }

        [return: MaybeNull]
        public static TItem FirstOrDefault<TItem, TList>(this ItemOrList<TItem, TList> itemOrList)
            where TItem : class?
            where TList : class, IReadOnlyList<TItem>
        {
            if (itemOrList.Item != null)
                return itemOrList.Item;
            return itemOrList.List?[0];
        }

        #endregion
    }
}