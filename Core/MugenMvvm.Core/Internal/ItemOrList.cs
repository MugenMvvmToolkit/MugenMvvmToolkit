using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace MugenMvvm.Internal
{
    [StructLayout(LayoutKind.Auto)]
    public readonly struct ItemOrList<TItem, TList>
        where TList : class, IReadOnlyList<TItem>
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
            var count = list.Count;
            if (count == 0)
            {
                List = default;
                Item = default!;
            }
            else if (count == 1)
            {
                List = default;
                Item = list[0];
            }
            else
            {
                List = list;
                Item = default!;
            }
        }

        #endregion

        #region Methods

        public ItemOrList<TItem, TNewList> Cast<TNewList>() where TNewList : class, IReadOnlyList<TItem>
        {
            if (List == null)
                return new ItemOrList<TItem, TNewList>(Item);
            return new ItemOrList<TItem, TNewList>((TNewList)(object)List);
        }

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