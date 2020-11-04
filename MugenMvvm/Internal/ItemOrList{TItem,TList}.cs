using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MugenMvvm.Extensions;

namespace MugenMvvm.Internal
{
    [StructLayout(LayoutKind.Auto)]
    public readonly struct ItemOrList<TItem, TList>
        where TList : class, IEnumerable<TItem>
    {
        #region Fields

        private readonly int _fixedCount;

        [MaybeNull]
        public readonly TItem Item;

        public readonly TList? List;

        #endregion

        #region Constructors

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ItemOrList([AllowNull] TItem item, bool hasItem)
        {
            Item = item!;
            List = null;
            _fixedCount = hasItem ? 1 : 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ItemOrList(TList? list)
        {
            if (list == null)
            {
                Item = default!;
                List = list;
                _fixedCount = 0;
                return;
            }

            if (list is TItem[] array)
            {
                _fixedCount = array.Length;
                if (_fixedCount > 1)
                {
                    Item = default!;
                    List = list;
                }
                else if (_fixedCount == 1)
                {
                    Item = array[0];
                    List = null;
                }
                else
                {
                    Item = default!;
                    List = null;
                }

                return;
            }

            int count;
            if (list is IReadOnlyList<TItem> readOnlyList)
            {
                count = readOnlyList.Count;
                if (count > 1)
                {
                    _fixedCount = 0;
                    Item = default!;
                    List = list;
                }
                else if (count == 1)
                {
                    _fixedCount = 1;
                    Item = readOnlyList[0];
                    List = null;
                }
                else
                {
                    _fixedCount = 0;
                    Item = default!;
                    List = null;
                }

                return;
            }

            count = list.CountEx();
            if (count > 1)
            {
                _fixedCount = 0;
                Item = default!;
                List = list;
            }
            else if (count == 1)
            {
                _fixedCount = 1;
                Item = list.ElementAt(0);
                List = null;
            }
            else
            {
                _fixedCount = 0;
                Item = default!;
                List = null;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ItemOrList(TList list, bool _)
        {
            Item = default!;
            List = list;
            _fixedCount = list is TItem[] array ? array.Length : 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ItemOrList(TItem item, TList? list, int fixedCount)
        {
            Item = item;
            List = list;
            _fixedCount = fixedCount;
        }

        #endregion

        #region Properties

        public bool IsEmpty
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _fixedCount == 0 && List == null;
        }

        public bool HasItem
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _fixedCount == 1 && List == null;
        }

        public int Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _fixedCount == 0 ? List.CountEx() : _fixedCount;
        }

        #endregion

        #region Methods

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ItemOrList<TItem, TList>(TItem item) => new ItemOrList<TItem, TList>(item, item != null);//note all value types will hasItem = true

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ItemOrList<TItem, TList>(TList? items) => new ItemOrList<TItem, TList>(items);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Enumerator GetEnumerator() => new Enumerator(this);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ItemOrList<TItem, TNewList> Cast<TNewList>() where TNewList : class, IEnumerable<TItem>
            => new ItemOrList<TItem, TNewList>(Item!, (TNewList?) (object?) List, _fixedCount);

        #endregion

        #region Nested types

        [StructLayout(LayoutKind.Auto)]
        public struct Enumerator : IDisposable
        {
            #region Fields

            private readonly TItem _item;
            private readonly IEnumerator<TItem>? _enumerator;
            private readonly IReadOnlyList<TItem>? _list;
            private readonly TItem[]? _array;
            private readonly int _count;
            private int _index;

            #endregion

            #region Constructors

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Enumerator(ItemOrList<TItem, TList> itemOrList)
            {
                _index = -1;
                var list = itemOrList.List;
                if (list == null)
                {
                    _item = itemOrList.Item!;
                    _count = itemOrList._fixedCount;
                    _array = null;
                    _enumerator = null;
                    _list = null;
                }
                else
                {
                    _item = default!;
                    _array = itemOrList.List as TItem[];
                    if (_array == null)
                    {
                        _list = itemOrList.List as IReadOnlyList<TItem>;
                        if (_list == null)
                        {
                            _array = null;
                            _enumerator = list.GetEnumerator();
                            _count = 0;
                        }
                        else
                        {
                            _array = null;
                            _enumerator = null;
                            _count = itemOrList.Count;
                        }
                    }
                    else
                    {
                        _list = null;
                        _enumerator = null;
                        _count = itemOrList.Count;
                    }
                }
            }

            #endregion

            #region Properties

            public TItem Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get
                {
                    if (_array != null)
                        return _array[_index];
                    if (_list != null)
                        return _list[_index];
                    return _enumerator == null ? _item! : _enumerator.Current;
                }
            }

            #endregion

            #region Implementation of interfaces

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Dispose() => _enumerator?.Dispose();

            #endregion

            #region Methods

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext()
            {
                if (_enumerator == null)
                    return ++_index < _count;
                return _enumerator.MoveNext();
            }

            #endregion
        }

        #endregion
    }
}