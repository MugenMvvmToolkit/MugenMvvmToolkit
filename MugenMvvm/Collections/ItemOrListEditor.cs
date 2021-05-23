using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MugenMvvm.Extensions;

namespace MugenMvvm.Collections
{
    [StructLayout(LayoutKind.Auto)]
    public struct ItemOrListEditor<T>
    {
        private bool _hasItem;
        private T? _item;
        private IList<T>? _list;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ItemOrListEditor(object? rawValue)
        {
            if (rawValue is IEnumerable<T> enumerable)
            {
                _item = default;
                _hasItem = false;
                if (enumerable is IList<T> l)
                    _list = l;
                else
                    _list = new List<T>(enumerable);
            }
            else
            {
                _list = null;
                if (rawValue == null)
                {
                    _hasItem = false;
                    _item = default!;
                }
                else
                {
                    _hasItem = true;
                    _item = (T) rawValue;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ItemOrListEditor(ItemOrIEnumerable<T> itemOrList, bool isRawList)
        {
            if (itemOrList.List != null)
            {
                _item = default;
                _hasItem = false;
                if (isRawList && itemOrList.List is IList<T> l)
                    _list = l;
                else
                    _list = new List<T>(itemOrList.List);
            }
            else
            {
                _list = null;
                _hasItem = itemOrList.HasItem;
                _item = itemOrList.Item!;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ItemOrListEditor(IList<T> iList)
        {
            _list = iList;
            _item = default!;
            _hasItem = false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ItemOrListEditor(List<T> list) : this(iList: list)
        {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ItemOrListEditor(T? item, IList<T>? list, bool hasItem)
        {
            _item = item!;
            _list = list;
            _hasItem = hasItem;
        }

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
            get => Count == 0;
        }

        public T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if ((uint) index >= (uint) Count)
                    ExceptionManager.ThrowIndexOutOfRangeCollection(nameof(index));

                return _list == null ? _item! : _list[index];
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ItemOrListEditor<T> FromRawValue(object? rawValue) => new(rawValue);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddRange(IEnumerable<T>? values) => AddRange(new ItemOrIEnumerable<T>(values));

        public void AddRange(ItemOrIEnumerable<T> value)
        {
            if (value.IsEmpty)
                return;

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
                        _list = new List<T>(2);
                        _list.AddRange(value.List);
                    }

                    return;
                }

                _list = new List<T>(2) {_item!};
                _item = default!;
                _hasItem = false;
            }

            if (value.List == null)
                _list.Add(value.Item!);
            else
                _list.AddRange(value.List);
        }

        public void Add(T item)
        {
            if (_list != null)
                _list.Add(item!);
            else if (_hasItem)
            {
                _list = new List<T>(2) {_item!, item!};
                _item = default;
                _hasItem = false;
            }
            else
            {
                _item = item;
                _hasItem = true;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(T item) => IndexOf(item) >= 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int IndexOf(T item)
        {
            if (_list != null)
                return _list.IndexOf(item);
            if (_hasItem && EqualityComparer<T>.Default.Equals(_item!, item))
                return 0;
            return -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(T item)
        {
            if (_list != null)
                return _list.Remove(item);

            if (EqualityComparer<T>.Default.Equals(_item!, item))
            {
                _item = default;
                _hasItem = false;
                return true;
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveAt(int index)
        {
            if ((uint) index >= (uint) Count)
                ExceptionManager.ThrowIndexOutOfRangeCollection(nameof(index));
            if (_list == null)
            {
                _item = default;
                _hasItem = false;
            }
            else
                _list.RemoveAt(index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            if (_list == null)
            {
                _item = default;
                _hasItem = false;
            }
            else
                _list.Clear();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal object? GetRawValueInternal()
        {
            if (_list == null)
                return _item;

            if (_list.Count > 1)
                return _list;
            return _list.Count == 0 ? null : _list[0];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ItemOrIReadOnlyList<T>(ItemOrListEditor<T> items) => items.ToItemOrList();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ItemOrIReadOnlyList<T> ToItemOrList()
        {
            if (_list == null)
                return new ItemOrIReadOnlyList<T>(_item, _hasItem);
            return new ItemOrIReadOnlyList<T>((IReadOnlyList<T>) _list);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IList<T> AsList()
        {
            if (_list != null)
                return _list;
            var result = new List<T>(2);
            if (_hasItem)
                result.Add(_item!);
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ItemOrArray<T> ToItemOrArray()
        {
            if (_list == null)
                return new ItemOrArray<T>(_item, _hasItem);
            if (_list.Count == 0)
                return default;
            if (_list.Count == 1)
                return new ItemOrArray<T>(_list[0], true);
            return new ItemOrArray<T>(_list.ToArray());
        }
    }
}