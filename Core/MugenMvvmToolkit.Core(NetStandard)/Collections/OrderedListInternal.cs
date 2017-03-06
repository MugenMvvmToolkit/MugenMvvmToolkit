#region Copyright

// ****************************************************************************
// <copyright file="OrderedListInternal.cs">
// Copyright (c) 2012-2017 Vyacheslav Volkov
// </copyright>
// ****************************************************************************
// <author>Vyacheslav Volkov</author>
// <email>vvs0205@outlook.com</email>
// <project>MugenMvvmToolkit</project>
// <web>https://github.com/MugenMvvmToolkit/MugenMvvmToolkit</web>
// <license>
// See license.txt in this solution or http://opensource.org/licenses/MS-PL
// </license>
// ****************************************************************************

#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Xml.Serialization;

namespace MugenMvvmToolkit.Collections
{
    [DebuggerDisplay("Count = {Count}")]
    [DataContract(IsReference = true, Namespace = ApplicationSettings.DataContractNamespace), Serializable]
    internal sealed class OrderedListInternal<TKey, TValue>
    {
        #region Fields

        [DataMember]
        internal Comparer<TKey> Comparer;

        [DataMember]
        internal TKey[] KeysInternal;

        [DataMember]
        internal int Size;

        [DataMember]
        internal TValue[] ValuesInternal;

        #endregion

        #region Constructors

        public OrderedListInternal()
        {
            KeysInternal = Empty.Array<TKey>();
            ValuesInternal = Empty.Array<TValue>();
            Size = 0;
            Comparer = Comparer<TKey>.Default;
        }

        public OrderedListInternal(OrderedListInternal<TKey, TValue> list)
        {
            KeysInternal = list.KeysInternal.ToArrayEx();
            ValuesInternal = list.ValuesInternal.ToArrayEx();
            Size = list.Size;
            Comparer = list.Comparer;
        }

        #endregion

        #region Properties

        public int Capacity
        {
            get { return KeysInternal.Length; }
            set
            {
                if (value == KeysInternal.Length)
                    return;
                if (value < Size)
                    throw ExceptionManager.CapacityLessThanCollection("Capacity");
                if (value > 0)
                {
                    var keyArray = new TKey[value];
                    var objArray = new TValue[value];
                    if (Size > 0)
                    {
                        Array.Copy(KeysInternal, 0, keyArray, 0, Size);
                        Array.Copy(ValuesInternal, 0, objArray, 0, Size);
                    }
                    KeysInternal = keyArray;
                    ValuesInternal = objArray;
                }
                else
                {
                    KeysInternal = Empty.Array<TKey>();
                    ValuesInternal = Empty.Array<TValue>();
                }
            }
        }

        public TKey[] Keys => KeysInternal;

        public TValue[] Values => ValuesInternal;

        public int Count => Size;

        #endregion

        #region Methods

        public int Add(TKey key, TValue value)
        {
            Should.NotBeNull(key, nameof(key));
            int num = Array.BinarySearch(KeysInternal, 0, Size, key, Comparer);
            if (num >= 0)
                throw ExceptionManager.DuplicateItemCollection(value);
            return Insert(~num, key, value);
        }

        public void Clear()
        {
            Array.Clear(KeysInternal, 0, Size);
            Array.Clear(ValuesInternal, 0, Size);
            Size = 0;
        }

        public int IndexOfKey(TKey key)
        {
            if (key == null)
                return -1;
            int num = Array.BinarySearch(KeysInternal, 0, Size, key, Comparer);
            if (num < 0)
                return -1;
            return num;
        }

        public int IndexOfValue(TValue value)
        {
            return Array.IndexOf(ValuesInternal, value, 0, Size);
        }

        public TKey GetKey(int index)
        {
            if (index >= Size)
                throw ExceptionManager.IntOutOfRangeCollection("index");
            return KeysInternal[index];
        }

        public TValue GetValue(int index)
        {
            if (index >= Size)
                throw ExceptionManager.IntOutOfRangeCollection("index");
            return ValuesInternal[index];
        }

        public void RemoveAt(int index)
        {
            if (index < 0 || index >= Size)
                throw ExceptionManager.IntOutOfRangeCollection("index");
            --Size;
            if (index < Size)
            {
                Array.Copy(KeysInternal, index + 1, KeysInternal, index, Size - index);
                Array.Copy(ValuesInternal, index + 1, ValuesInternal, index, Size - index);
            }
            KeysInternal[Size] = default(TKey);
            ValuesInternal[Size] = default(TValue);
        }

        private void EnsureCapacity(int min)
        {
            int num = KeysInternal.Length == 0 ? 4 : KeysInternal.Length * 2;
            if (num < min)
                num = min;
            Capacity = num;
        }

        private int Insert(int index, TKey key, TValue value)
        {
            if (Size == KeysInternal.Length)
                EnsureCapacity(Size + 1);
            if (index < Size)
            {
                Array.Copy(KeysInternal, index, KeysInternal, index + 1, Size - index);
                Array.Copy(ValuesInternal, index, ValuesInternal, index + 1, Size - index);
            }
            KeysInternal[index] = key;
            ValuesInternal[index] = value;
            ++Size;
            return index;
        }

        #endregion
    }

    [DebuggerDisplay("Count = {Count}")]
    [DataContract(IsReference = true, Namespace = ApplicationSettings.DataContractNamespace), Serializable]
    internal sealed class OrderedListInternal<T> : IList<T>, IList
    {
        #region Fields

        [DataMember]
        internal IComparer<T> ComparerInternal;

        [DataMember]
        internal T[] Items;

        [DataMember]
        internal int Size;

        [XmlIgnore, NonSerialized, IgnoreDataMember]
        private object _syncRoot;

        #endregion

        #region Constructors

        public OrderedListInternal()
            : this(null)
        {
        }

        public OrderedListInternal(IComparer<T> comparer = null)
        {
            Size = 0;
            Items = Empty.Array<T>();
            if (comparer == null)
                comparer = Comparer<T>.Default;
            ComparerInternal = comparer;
        }

        public OrderedListInternal(IEnumerable<T> items, IComparer<T> comparer = null)
            : this(comparer)
        {
            foreach (T item in items)
                Add(item);
        }

        #endregion

        #region Properties

        public IComparer<T> Comparer => ComparerInternal;

        public int Capacity
        {
            get { return Items.Length; }
            set
            {
                if (value == Items.Length)
                    return;
                if (value < Size)
                    throw ExceptionManager.CapacityLessThanCollection("Capacity");
                if (value > 0)
                {
                    var keyArray = new T[value];
                    if (Size > 0)
                    {
                        Array.Copy(Items, 0, keyArray, 0, Size);
                    }
                    Items = keyArray;
                }
                else
                {
                    Items = Empty.Array<T>();
                }
            }
        }

        #endregion

        #region Methods

        public int GetInsertIndex(T item)
        {
            Should.NotBeNull(item, nameof(item));
            int num = Array.BinarySearch(Items, 0, Size, item, ComparerInternal);
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
            if (index < Size)
            {
                Array.Copy(Items, index, Items, index + 1, Size - index);
            }
            Items[index] = item;
            ++Size;
            return index;
        }

        private void EnsureCapacity(int min)
        {
            int num = Items.Length == 0 ? 4 : Items.Length * 2;
            if (num < min)
                num = min;
            Capacity = num;
        }

        #endregion

        #region Implementation of interfaces

        int IList.Add(object value)
        {
            Should.NotBeNull(value, nameof(value));
            return Add((T)value);
        }

        bool IList.Contains(object value)
        {
            if (SynchronizedNotifiableCollection<T>.IsCompatibleObject(value))
                return Contains((T)value);
            return false;
        }

        void IList.Clear()
        {
            Clear();
        }

        int IList.IndexOf(object value)
        {
            return IndexOf((T)value);
        }

        void IList.Insert(int index, object value)
        {
            Insert(index, (T)value);
        }

        void IList.Remove(object value)
        {
            if (SynchronizedNotifiableCollection<T>.IsCompatibleObject(value))
                Remove((T)value);
        }

        void IList.RemoveAt(int index)
        {
            RemoveAt(index);
        }

        object IList.this[int index]
        {
            get { return this[index]; }
            set { this[index] = (T)value; }
        }

        bool IList.IsReadOnly => IsReadOnly;

        bool IList.IsFixedSize => false;

        public void CopyTo(Array array, int index)
        {
            Array.Copy(Items, 0, array, index, Size);
        }

        int ICollection.Count => Count;

        object ICollection.SyncRoot
        {
            get
            {
                if (_syncRoot == null)
                    Interlocked.CompareExchange(ref _syncRoot, new object(), null);
                return _syncRoot;
            }
        }

        bool ICollection.IsSynchronized => false;

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
            int indexOf = IndexOfObject(item);
            if (indexOf == -1)
                return false;
            RemoveAt(indexOf);
            return true;
        }

        public int Count => Size;

        public bool IsReadOnly => false;

        public int IndexOf(T item)
        {
            return IndexOfObject(item);
        }

        public void Insert(int index, T item)
        {
            if (index > Size)
                throw ExceptionManager.IntOutOfRangeCollection("index");
            int insertIndex = GetInsertIndex(item);
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
            if (index < Size)
            {
                Array.Copy(Items, index + 1, Items, index, Size - index);
            }
            Items[Size] = default(T);
        }

        public T this[int index]
        {
            get
            {
                if (index >= Size)
                    throw ExceptionManager.IntOutOfRangeCollection("index");
                return Items[index];
            }
            set { Should.MethodBeSupported(false, "this[int index]"); }
        }

        public int Add(T item)
        {
            Should.NotBeNull(item, nameof(item));
            return InsertInternal(GetInsertIndex(item), item);
        }

        #endregion
    }
}
