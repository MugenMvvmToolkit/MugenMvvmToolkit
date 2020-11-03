using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MugenMvvm.Extensions;

namespace MugenMvvm.Internal
{
    [StructLayout(LayoutKind.Auto)]
    public struct ItemOrListEditor<TItem, TList>
        where TList : class, IList<TItem>
    {
        #region Fields

        private readonly Func<TList> _getNewList;
        private TItem _item;
        private TList? _list;
        private bool _hasItem;

        #endregion

        #region Constructors

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ItemOrListEditor(Func<TList> getNewList) : this(default!, null, false, getNewList)
        {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ItemOrListEditor([AllowNull] TItem item, TList? list, bool hasItem, Func<TList> getNewList)
        {
            Should.NotBeNull(getNewList, nameof(getNewList));
            _item = item!;
            _list = list;
            _hasItem = hasItem;
            _getNewList = getNewList;
        }

        #endregion

        #region Properties

        public int Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (_list != null)
                    return _list.Count;
                return _hasItem ? 1 : 0;
            }
        }

        public bool IsEmpty
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => !_hasItem && _list == null;
        }

        public TItem this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if ((uint) index >= (uint) Count)
                    ExceptionManager.ThrowIndexOutOfRangeCollection(nameof(index));

                return _list == null ? _item : _list[index];
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                if ((uint) index >= (uint) Count)
                    ExceptionManager.ThrowIndexOutOfRangeCollection(nameof(index));

                if (_list != null)
                    _list[index] = value;
                else
                    _item = value;
            }
        }

        #endregion

        #region Methods

        public ItemOrListEditor<TItem, TList> AddRange(IEnumerable<TItem>? values) => AddRange(new ItemOrList<TItem, IEnumerable<TItem>>(values));

        public ItemOrListEditor<TItem, TList> AddRange<T>(ItemOrList<TItem, T> value) where T : class, IEnumerable<TItem>
        {
            if (value.IsEmpty)
                return this;

            if (_list == null)
            {
                if (!_hasItem)
                {
                    if (value.List == null)
                    {
                        _item = value.Item!;
                        _hasItem = true;
                    }
                    else
                    {
                        _list = _getNewList();
                        _list.AddRange(value.List);
                    }

                    return this;
                }

                _list = _getNewList();
                _list.Add(_item);
                _item = default!;
                _hasItem = false;
            }

            if (value.List == null)
                _list.Add(value.Item!);
            else
                _list.AddRange(value.List);
            return this;
        }

        public ItemOrListEditor<TItem, TList> Add(TItem item)
        {
            if (_list != null)
                _list.Add(item!);
            else if (_hasItem)
            {
                _list = _getNewList();
                _list.Add(_item);
                _list.Add(item!);
                _item = default!;
                _hasItem = false;
            }
            else
            {
                _item = item!;
                _hasItem = true;
            }

            return this;
        }

        public bool Remove(TItem item)
        {
            if (_list != null)
                return _list.Remove(item);

            if (EqualityComparer<TItem>.Default.Equals(_item, item))
            {
                _item = default!;
                _hasItem = false;
                return true;
            }

            return false;
        }

        public void RemoveAt(int index)
        {
            if ((uint) index >= (uint) Count)
                ExceptionManager.ThrowIndexOutOfRangeCollection(nameof(index));
            if (_list == null)
            {
                _item = default!;
                _hasItem = false;
            }
            else
                _list.RemoveAt(index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal object? GetRawValueInternal()
        {
            if (_list == null)
                return _item;

            if (_list.Count > 1)
                return _list;
            return _list.Count == 0 ? null : (object?) _list[0];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ItemOrList<TItem, TList> ToItemOrList() => ToItemOrList<TList>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ItemOrList<TItem, TNewList> ToItemOrList<TNewList>() where TNewList : class, IEnumerable<TItem>
        {
            if (_list == null)
                return new ItemOrList<TItem, TNewList>(_item, _hasItem);

            if (_list.Count > 1)
                return new ItemOrList<TItem, TNewList>((TNewList) (object) _list, true);
            return _list.Count == 0 ? default! : new ItemOrList<TItem, TNewList>(_list[0], true);
        }

        #endregion
    }
}