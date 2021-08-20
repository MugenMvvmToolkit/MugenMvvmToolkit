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
        private int _defaultCapacity;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ItemOrListEditor(int capacity)
        {
            _defaultCapacity = capacity;
            _list = null;
            _item = default!;
            _hasItem = false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ItemOrListEditor(T item)
        {
            _defaultCapacity = 0;
            _list = null;
            _item = item;
            _hasItem = true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ItemOrListEditor(IList<T>? iList)
        {
            _defaultCapacity = 0;
            _list = iList;
            _item = default!;
            _hasItem = false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ItemOrListEditor(T? item, IList<T>? list, bool hasItem)
        {
            _defaultCapacity = 0;
            _item = item!;
            _list = list;
            _hasItem = hasItem;
        }

        public ItemOrListEditor(ItemOrIEnumerable<T> itemOrList, bool isRawList)
        {
            _defaultCapacity = 0;
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

        public readonly int Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (_list != null)
                    return _list.Count;
                return _hasItem ? 1 : 0;
            }
        }

        public readonly bool IsEmpty
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Count == 0;
        }

        public int DefaultCapacity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            readonly get => _defaultCapacity == 0 ? 4 : _defaultCapacity;
            set => _defaultCapacity = value;
        }

        public T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            readonly get
            {
                if ((uint)index >= (uint)Count)
                    ExceptionManager.ThrowIndexOutOfRangeCollection(nameof(index));

                return _list == null ? _item! : _list[index];
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                if ((uint)index >= (uint)Count)
                    ExceptionManager.ThrowIndexOutOfRangeCollection(nameof(index));

                if (_list != null)
                    _list[index] = value;
                else
                    _item = value;
            }
        }

        public static ItemOrListEditor<T> FromRawValue(object? rawValue)
        {
            if (rawValue == null)
                return default;
            if (rawValue is IList<T> list)
                return new ItemOrListEditor<T>(list);
            if (rawValue is IEnumerable<T> enumerable)
                return new ItemOrListEditor<T>(new List<T>(enumerable));
            return new ItemOrListEditor<T>((T)rawValue, null, true);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ItemOrIReadOnlyList<T>(ItemOrListEditor<T> items) => items.ToItemOrList();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ItemOrListEnumerator<T> GetEnumerator() => new(ToItemOrList());

        public void AddRange(List<T>? values) => AddRange(ItemOrIEnumerable.FromList(values));

        public void AddRange(IEnumerable<T>? values)
        {
            if (values == null)
                return;

            if (values is IReadOnlyList<T> list)
                AddRange(ItemOrIEnumerable.FromList(list));
            else
            {
                foreach (var value in values)
                    Add(value);
            }
        }

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
                        _list = new List<T>(DefaultCapacity);
                        _list.AddRange(value.List);
                    }

                    return;
                }

                _list = new List<T>(DefaultCapacity) { _item! };
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
                _list = new List<T>(DefaultCapacity) { _item!, item! };
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
        public readonly bool Contains(T item) => IndexOf(item) >= 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int IndexOf(T item)
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
            if ((uint)index >= (uint)Count)
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
        internal readonly object? GetRawValueInternal()
        {
            if (_list == null)
                return _item;

            if (_list.Count > 1)
                return _list;
            return _list.Count == 0 ? null : _list[0];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ItemOrIReadOnlyList<T> ToItemOrList()
        {
            if (_list == null)
                return ItemOrIReadOnlyList.FromItem(_item, _hasItem);
            return ItemOrIReadOnlyList.FromList((IReadOnlyList<T>)_list);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly IList<T> AsList()
        {
            if (_list != null)
                return _list;
            var result = new List<T>(2);
            if (_hasItem)
                result.Add(_item!);
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ItemOrArray<T> ToItemOrArray()
        {
            if (_list == null)
                return ItemOrArray.FromItem(_item, _hasItem);
            if (_list.Count == 0)
                return default;
            if (_list.Count == 1)
                return new ItemOrArray<T>(_list[0]);
            var array = _list.ToArray();
            return new ItemOrArray<T>(default, array, array.Length);
        }
    }
}