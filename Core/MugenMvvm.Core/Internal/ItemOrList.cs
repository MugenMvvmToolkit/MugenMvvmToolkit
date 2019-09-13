using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace MugenMvvm.Internal
{
    [StructLayout(LayoutKind.Auto)]
    public readonly struct ItemOrList<TItem, TList>
        where TList : class, IEnumerable<TItem>
    {
        #region Fields

        public readonly TItem Item;
        public readonly TList? List;

        #endregion

        #region Constructors

        public ItemOrList(TItem item)
        {
            Item = item;
            List = null;
        }

        public ItemOrList(TList list)
        {
            List = list;
            Item = default;
        }

        #endregion

        #region Properties

        public bool IsList => List != null;

        #endregion

        #region Methods

        public static implicit operator ItemOrList<TItem, TList>(TItem item)
        {
            return new ItemOrList<TItem, TList>(item);
        }

        public static implicit operator ItemOrList<TItem, TList>(TList items)
        {
            return new ItemOrList<TItem, TList>(items);
        }

        #endregion
    }
}