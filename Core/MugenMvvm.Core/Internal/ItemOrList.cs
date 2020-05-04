using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MugenMvvm.Internal
{
    [StructLayout(LayoutKind.Auto)]
    public readonly struct ItemOrList<TItem, TList>
        where TList : class, IEnumerable<TItem>
    {
        #region Fields

        private static readonly TItem[] SingleItemArray = new TItem[1];

        [MaybeNull]
        public readonly TItem Item;
        public readonly TList? List;

        #endregion

        #region Constructors

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ItemOrList([AllowNull] TItem item)
        {
            Item = item!;
            List = null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ItemOrList(TList? list)
        {
            if (list == null)
            {
                List = default;
                Item = default!;
                return;
            }

            if (list is TItem[] array)
            {
                var count = array.Length;
                if (count == 0)
                {
                    List = default;
                    Item = default!;
                }
                else if (count == 1)
                {
                    List = default;
                    Item = array[0];
                }
                else
                {
                    List = list;
                    Item = default!;
                }
            }
            else if (list is IReadOnlyList<TItem> l)
            {
                var count = l.Count;
                if (count == 0)
                {
                    List = default;
                    Item = default!;
                }
                else if (count == 1)
                {
                    List = default;
                    Item = l[0];
                }
                else
                {
                    List = list;
                    Item = default!;
                }
            }
            else if (list is ICollection<TItem> c)
            {
                var count = c.Count;
                if (count == 0)
                {
                    List = default;
                    Item = default!;
                }
                else if (count == 1)
                {
                    c.CopyTo(SingleItemArray, 0);
                    List = default;
                    Item = SingleItemArray[0];
                    SingleItemArray[0] = default!;
                }
                else
                {
                    List = list;
                    Item = default!;
                }
            }
            else
            {
                var count = list.Count();
                if (count == 0)
                {
                    List = default;
                    Item = default!;
                }
                else if (count == 1)
                {
                    List = default;
                    Item = list.FirstOrDefault();
                }
                else
                {
                    List = list;
                    Item = default!;
                }
            }
        }

        #endregion

        #region Methods

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ItemOrList<TItem, TList> FromRawValue(object? value)
        {
            if (value is TList list)
                return new ItemOrList<TItem, TList>(list);
            return new ItemOrList<TItem, TList>((TItem)value!);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ItemOrList<TItem, TNewList> Cast<TNewList>() where TNewList : class, IReadOnlyList<TItem>
        {
            if (List == null)
                return new ItemOrList<TItem, TNewList>(Item);
            return new ItemOrList<TItem, TNewList>((TNewList)(object)List);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ItemOrList<TItem, TList>([AllowNull] TItem item)
        {
            return new ItemOrList<TItem, TList>(item);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ItemOrList<TItem, TList>(TList? items)
        {
            return new ItemOrList<TItem, TList>(items);
        }

        #endregion
    }
}