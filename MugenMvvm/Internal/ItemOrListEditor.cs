using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace MugenMvvm.Internal
{
    public static class ItemOrListEditor
    {
        #region Methods

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ItemOrListEditor<TItem, List<TItem>> Get<TItem>() where TItem : class
        {
            return Get<TItem, List<TItem>>(() => new List<TItem>());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ItemOrListEditor<TItem, List<TItem>> Get<TItem>(Func<TItem, bool> isEmpty)
        {
            return Get(isEmpty, () => new List<TItem>());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ItemOrListEditor<TItem, TList> Get<TItem, TList>(Func<TList> getNewList)
            where TItem : class
            where TList : class, IList<TItem>
        {
            return Get<TItem, TList>(i => i == null, getNewList);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ItemOrListEditor<TItem, TList> Get<TItem, TList>(Func<TItem, bool> isEmpty, Func<TList> getNewList)
            where TList : class, IList<TItem>
        {
            return new ItemOrListEditor<TItem, TList>(isEmpty, getNewList);
        }

        #endregion
    }
}