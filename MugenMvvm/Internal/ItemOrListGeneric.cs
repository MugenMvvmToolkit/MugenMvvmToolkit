using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MugenMvvm.Internal
{
    [StructLayout(LayoutKind.Auto)]
    public readonly struct ItemOrList<TItem, TList>
        where TList : class, IEnumerable<TItem>
    {
        #region Fields

        [MaybeNull]
        public readonly TItem Item;
        public readonly TList? List;

        #endregion

        #region Constructors

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ItemOrList([AllowNull] TItem item)
        {
            Item = item!;
            List = null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ItemOrList(TList? list)
        {
            List = list;
            Item = default!;
        }

        #endregion

        #region Methods

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Deconstruct([MaybeNull] out TItem item, [MaybeNull] out TList list)
        {
            item = Item;
            list = List;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ItemOrList<TItem, TNewList> Cast<TNewList>() where TNewList : class, IEnumerable<TItem>
        {
            if (List == null)
                return new ItemOrList<TItem, TNewList>(Item);
            return new ItemOrList<TItem, TNewList>((TNewList) (object) List);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ItemOrList<TItem, TList>([AllowNull] TItem item)
        {
            return new ItemOrList<TItem, TList>(item);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ItemOrList<TItem, TList>(TList? items)
        {
            return ItemOrList.FromList<TItem, TList>(items);
        }

        #endregion
    }
}