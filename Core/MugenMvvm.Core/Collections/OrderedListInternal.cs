using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using MugenMvvm.Attributes;
using MugenMvvm.Constants;

namespace MugenMvvm.Collections
{
    [DebuggerDisplay("Count = {Count}")]
    [DataContract(IsReference = true, Namespace = BuildConstants.DataContractNamespace)]
    [Serializable]
    internal sealed class OrderedListInternal<T> : IList<T>, IReadOnlyList<T>
    {
        #region Fields

        [DataMember]
        internal IComparer<T> ComparerInternal;

        [DataMember]
        internal T[] Items;

        [DataMember]
        internal int Size;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public OrderedListInternal()
            : this(null)
        {
        }

        public OrderedListInternal(IComparer<T>? comparer = null)
        {
            Size = 0;
            Items = Default.EmptyArray<T>();
            if (comparer == null)
                comparer = Comparer<T>.Default;
            ComparerInternal = comparer;
        }

        public OrderedListInternal(IEnumerable<T> items, IComparer<T>? comparer = null)
            : this(comparer)
        {
            foreach (var item in items)
                Add(item);
        }

        #endregion

        #region Properties

        public IComparer<T> Comparer => ComparerInternal;

        public int Capacity
        {
            get => Items.Length;
            set
            {
                if (value == Items.Length)
                    return;
                if (value < Size)
                    throw ExceptionManager.CapacityLessThanCollection("Capacity");
                if (value > 0)
                {
                    var keyArray = new T[value];
                    if (Size > 0) Array.Copy(Items, 0, keyArray, 0, Size);
                    Items = keyArray;
                }
                else
                    Items = Default.EmptyArray<T>();
            }
        }

        public int Count => Size;

        public bool IsReadOnly => false;

        public T this[int index]
        {
            get
            {
                if (index >= Size)
                    throw ExceptionManager.IntOutOfRangeCollection("index");
                return Items[index];
            }
            set => Should.MethodBeSupported(false, "this[int index]");
        }

        #endregion

        #region Implementation of interfaces

        public IEnumerator<T> GetEnumerator()
        {
            return Items.Take(Size).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        void ICollection<T>.Add(T item)
        {
            Add(item);
        }

        public void Clear()
        {
            Array.Clear(Items, 0, Size);
            Size = 0;
        }

        public bool Contains(T item)
        {
            return IndexOfObject(item) >= 0;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            Array.Copy(Items, 0, array, arrayIndex, Size);
        }

        public bool Remove(T item)
        {
            Should.NotBeNull(item, nameof(item));
            var indexOf = IndexOfObject(item);
            if (indexOf == -1)
                return false;
            RemoveAt(indexOf);
            return true;
        }

        public int IndexOf(T item)
        {
            return IndexOfObject(item);
        }

        public void Insert(int index, T item)
        {
            if (index > Size)
                throw ExceptionManager.IntOutOfRangeCollection("index");
            var insertIndex = GetInsertIndex(item);
            if (insertIndex == index)
            {
                InsertInternal(index, item);
                return;
            }

            if (index == 0 && Size == 0)
            {
                InsertInternal(index, item);
                return;
            }


            if (index == Size)
                CheckInsertItem(Items[Size - 1], item);
            else if (index == 0)
                CheckInsertItem(Items[0], item);
            else
            {
                if (!CheckInsertItem(Items[index], item, false) && !CheckInsertItem(Items[index - 1], item, false))
                    CheckInsertItem(Items[index + 1], item);
            }

            InsertInternal(index, item);
        }

        public void RemoveAt(int index)
        {
            if (index < 0 || index >= Size)
                throw ExceptionManager.IntOutOfRangeCollection("index");
            --Size;
            if (index < Size) Array.Copy(Items, index + 1, Items, index, Size - index);
            Items[Size] = default;
        }

        #endregion

        #region Methods

        public int GetInsertIndex(T item)
        {
            Should.NotBeNull(item, nameof(item));
            var num = Array.BinarySearch(Items, 0, Size, item, ComparerInternal);
            if (num >= 0)
                return num;
            return ~num;
        }

        private int IndexOfObject(T item)
        {
            return Array.IndexOf(Items, item, 0, Size);
        }

        private bool CheckInsertItem(T oldItem, T newItem, bool throwException = true)
        {
            if (Comparer.Compare(oldItem, newItem) != 0)
            {
                Should.BeValid("index", !throwException);
                return false;
            }

            return true;
        }

        private int InsertInternal(int index, T item)
        {
            if (Size == Items.Length)
                EnsureCapacity(Size + 1);
            if (index < Size) Array.Copy(Items, index, Items, index + 1, Size - index);
            Items[index] = item;
            ++Size;
            return index;
        }

        private void EnsureCapacity(int min)
        {
            var num = Items.Length == 0 ? 4 : Items.Length * 2;
            if (num < min)
                num = min;
            Capacity = num;
        }

        public int Add(T item)
        {
            Should.NotBeNull(item, nameof(item));
            return InsertInternal(GetInsertIndex(item), item);
        }

        public void Reorder()
        {
            Array.Sort(Items, 0, Size, ComparerInternal);
        }

        #endregion
    }
}