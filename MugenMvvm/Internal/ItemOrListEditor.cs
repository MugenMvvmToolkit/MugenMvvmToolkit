using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace MugenMvvm.Internal
{
    public static class ItemOrListEditor
    {
        #region Methods

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ItemOrListEditor<TItem, List<TItem>> Get<TItem>() => Get<TItem, List<TItem>>(() => new List<TItem>());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ItemOrListEditor<TItem, TList> Get<TItem, TList>(Func<TList> getNewList) where TList : class, IList<TItem> => new ItemOrListEditor<TItem, TList>(getNewList);

        #endregion
    }
}