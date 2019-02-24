using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using MugenMvvm.Attributes;
using MugenMvvm.Constants;

namespace MugenMvvm.Collections
{
    [Serializable]
    [DataContract(Namespace = BuildConstants.DataContractNamespace)]
    [Preserve(Conditional = true, AllMembers = true)]
    public class LightArrayList<T>
    {
        #region Fields

        private const int DefaultCapacity = 4;

        #endregion

        #region Constructors

        public LightArrayList()
        {
            Items = Default.EmptyArray<T>();
        }

        public LightArrayList(uint capacity)
        {
            Items = capacity == 0 ? Default.EmptyArray<T>() : new T[capacity];
        }

        public LightArrayList(IEnumerable<T> collection)
        {
            Should.NotBeNull(collection, nameof(collection));
            if (collection is ICollection<T> items)
            {
                var count = items.Count;
                if (count == 0)
                    Items = Default.EmptyArray<T>();
                else
                {
                    Items = new T[count];
                    items.CopyTo(Items, 0);
                    Size = count;
                }
            }
            else
            {
                Size = 0;
                Items = Default.EmptyArray<T>();
                foreach (var obj in collection)
                    Add(obj);
            }
        }

        #endregion

        #region Properties

        [field: DataMember(Name = "S")]
        protected int Size { get; set; }

        [field: DataMember(Name = "I")]
        protected T[] Items { get; private set; }

        [IgnoreDataMember, XmlIgnore]
        protected uint Capacity
        {
            get => (uint)Items.Length;
            set
            {
                if (value < Size)
                    throw ExceptionManager.CapacityLessThanCollection(nameof(Capacity));
                if (value == Items.Length)
                    return;
                if (value > 0)
                {
                    var objArray = new T[value];
                    if (Size > 0)
                        Array.Copy(Items, 0, objArray, 0, Size);
                    Items = objArray;
                }
                else
                    Items = Default.EmptyArray<T>();
            }
        }

        #endregion

        #region Methods

        public T[] GetItemsWithLock(out int size)
        {
            lock (this)
            {
                return GetItems(out size);
            }
        }

        public T[] GetItems(out int size)
        {
            size = Size;
            return Items;
        }

        public void AddWithLock(T item)
        {
            lock (this)
            {
                Add(item);
            }
        }

        public void Add(T item)
        {
            AddInternal(item);
        }

        public bool ContainsWithLock(T item)
        {
            var items = GetItemsWithLock(out var size);
            return IndexOfInternal(items, item, size) >= 0;
        }

        public bool Contains(T item)
        {
            return IndexOfInternal(Items, item, Size) >= 0;
        }

        public bool RemoveWithLock(T item)
        {
            lock (this)
            {
                return Remove(item);
            }
        }

        public bool Remove(T item)
        {
            var index = IndexOfInternal(Items, item, Size);
            if (index < 0)
                return false;
            RemoveAtInternal(index);
            return true;
        }

        public void ClearWithLock()
        {
            lock (this)
            {
                Clear();
            }
        }

        public void Clear()
        {
            ClearInternal();
        }

        public T[] ToArrayWithLock()
        {
            var items = GetItemsWithLock(out var size);
            return ToArrayInternal(items, size);
        }

        public T[] ToArray()
        {
            return ToArrayInternal(Items, Size);
        }

        protected virtual void AddInternal(T item)
        {
            if (Size == Items.Length)
                EnsureCapacity((uint)Size + 1);
            Items[Size++] = item;
        }

        protected virtual void RemoveAtInternal(int index)
        {
            if (index > Size)
                throw ExceptionManager.IndexOutOfRangeCollection("index");
            --Size;
            if (index < Size)
                Array.Copy(Items, index + 1, Items, index, Size - index);
            Items[Size] = default!;
        }

        protected virtual int IndexOfInternal(T[] items, T item, int size)
        {
            return Array.IndexOf(items, item, 0, size);
        }

        protected virtual void ClearInternal()
        {
            if (Size > 0)
            {
                Array.Clear(Items, 0, Size);
                Size = 0;
            }
        }

        protected virtual T[] ToArrayInternal(T[] items, int size)
        {
            if (size == 0)
                return Default.EmptyArray<T>();

            var result = new T[size];
            for (var i = 0; i < result.Length; i++)
                result[i] = items[i];

            return result;
        }

        protected void EnsureCapacity(uint min)
        {
            if (Items.Length >= min)
                return;
            var num = (uint)(Items.Length == 0 ? DefaultCapacity : Items.Length * 2);
            if (num < min)
                num = min;
            Capacity = num;
        }

        #endregion
    }
}