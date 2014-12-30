#region Copyright

// ****************************************************************************
// <copyright file="OrderedListInternal.cs">
// Copyright (c) 2012-2015 Vyacheslav Volkov
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
    /// <summary>
    ///     Represents a collection of key/value pairs that are sorted by key based on the associated
    ///     <see
    ///         cref="T:System.Collections.Generic.IComparer`1" />
    ///     implementation.
    /// </summary>
    /// <typeparam name="TKey">
    ///     The type of keys in the collection.
    /// </typeparam>
    /// <typeparam name="TValue">
    ///     The type of values in the collection.
    /// </typeparam>
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

        /// <summary>
        ///     Initializes a new instance of the <see cref="OrderedListInternal{TKey,TValue}" /> class that is empty, has the
        ///     default initial capacity, and uses the default
        ///     <see
        ///         cref="T:System.Collections.Generic.IComparer`1" />
        ///     .
        /// </summary>
        public OrderedListInternal()
        {
            KeysInternal = Empty.Array<TKey>();
            ValuesInternal = Empty.Array<TValue>();
            Size = 0;
            Comparer = Comparer<TKey>.Default;
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets or sets the number of elements that the <see cref="OrderedListInternal{TKey,TValue}" /> can contain.
        /// </summary>
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

        /// <summary>
        ///     Gets a collection containing the keys in the <see cref="OrderedListInternal{TKey,TValue}" />.
        /// </summary>
        public TKey[] Keys
        {
            get { return KeysInternal; }
        }

        /// <summary>
        ///     Gets a collection containing the values in the <see cref="OrderedListInternal{TKey,TValue}" />.
        /// </summary>
        public TValue[] Values
        {
            get { return ValuesInternal; }
        }

        /// <summary>
        ///     Gets the number of key/value pairs contained in the <see cref="OrderedListInternal{TKey,TValue}" />.
        /// </summary>
        public int Count
        {
            get { return Size; }
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Adds an element with the specified key and value into the <see cref="OrderedListInternal{TKey,TValue}" />.
        /// </summary>
        public int Add(TKey key, TValue value)
        {
            Should.NotBeNull(key, "key");
            int num = Array.BinarySearch(KeysInternal, 0, Size, key, Comparer);
            if (num >= 0)
                throw ExceptionManager.DuplicateItemCollection(value);
            return Insert(~num, key, value);
        }

        /// <summary>
        ///     Removes all elements from the <see cref="OrderedListInternal{TKey,TValue}" />.
        /// </summary>
        public void Clear()
        {
            Array.Clear(KeysInternal, 0, Size);
            Array.Clear(ValuesInternal, 0, Size);
            Size = 0;
        }

        /// <summary>
        ///     Searches for the specified key and returns the zero-based index within the entire
        ///     <see
        ///         cref="OrderedListInternal{TKey,TValue}" />
        ///     .
        /// </summary>
        public int IndexOfKey(TKey key)
        {
            Should.NotBeNull(key, "key");
            int num = Array.BinarySearch(KeysInternal, 0, Size, key, Comparer);
            if (num < 0)
                return -1;
            return num;
        }

        /// <summary>
        ///     Searches for the specified value and returns the zero-based index of the first occurrence within the entire
        ///     <see
        ///         cref="OrderedListInternal{TKey,TValue}" />
        ///     .
        /// </summary>
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

        /// <summary>
        ///     Removes the element at the specified index of the <see cref="OrderedListInternal{TKey,TValue}" />.
        /// </summary>
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

    /// <summary>
    ///     Represents a collection of objects that are sorted by key based on the associated
    ///     <see
    ///         cref="T:System.Collections.Generic.IComparer`1" />
    ///     implementation. Duplicate items (items that compare equal to each other) are allows in an OrderedListInternal.
    /// </summary>
    /// <typeparam name="T">
    ///     The type of item in the collection.
    /// </typeparam>
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

        /// <summary>
        ///     Initializes a new instance of the <see cref="OrderedListInternal{T}" /> class.
        /// </summary>
        public OrderedListInternal()
            : this(null)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="OrderedListInternal{T}" /> class.
        /// </summary>
        public OrderedListInternal(IComparer<T> comparer = null)
        {
            Size = 0;
            Items = Empty.Array<T>();
            if (comparer == null)
                comparer = Comparer<T>.Default;
            ComparerInternal = comparer;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="OrderedListInternal{T}" /> class.
        /// </summary>
        public OrderedListInternal(IEnumerable<T> items, IComparer<T> comparer = null)
            : this(comparer)
        {
            foreach (T item in items)
                Add(item);
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the comparer.
        /// </summary>
        public IComparer<T> Comparer
        {
            get { return ComparerInternal; }
        }

        /// <summary>
        ///     Gets or sets the number of elements that the <see cref="OrderedListInternal{T}" /> can contain.
        /// </summary>
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
            Should.NotBeNull(item, "item");
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

        /// <summary>
        ///     Adds an item to the <see cref="T:System.Collections.IList" />.
        /// </summary>
        int IList.Add(object value)
        {
            Should.NotBeNull(value, "value");
            return Add((T)value);
        }

        /// <summary>
        ///     Determines whether the <see cref="T:System.Collections.IList" /> contains a specific value.
        /// </summary>
        bool IList.Contains(object value)
        {
            if (SynchronizedNotifiableCollection<T>.IsCompatibleObject(value))
                return Contains((T)value);
            return false;
        }

        /// <summary>
        ///     Removes all items from the <see cref="T:System.Collections.IList" />.
        /// </summary>
        void IList.Clear()
        {
            Clear();
        }

        /// <summary>
        ///     Determines the index of a specific item in the <see cref="T:System.Collections.IList" />.
        /// </summary>
        int IList.IndexOf(object value)
        {
            return IndexOf((T)value);
        }

        /// <summary>
        ///     Inserts an item to the <see cref="T:System.Collections.IList" /> at the specified index.
        /// </summary>
        void IList.Insert(int index, object value)
        {
            Insert(index, (T)value);
        }

        /// <summary>
        ///     Removes the first occurrence of a specific object from the <see cref="T:System.Collections.IList" />.
        /// </summary>
        void IList.Remove(object value)
        {
            if (SynchronizedNotifiableCollection<T>.IsCompatibleObject(value))
                Remove((T)value);
        }

        /// <summary>
        ///     Removes the <see cref="T:System.Collections.IList" /> item at the specified index.
        /// </summary>
        void IList.RemoveAt(int index)
        {
            RemoveAt(index);
        }

        /// <summary>
        ///     Gets or sets the element at the specified index.
        /// </summary>
        object IList.this[int index]
        {
            get { return this[index]; }
            set { this[index] = (T)value; }
        }

        /// <summary>
        ///     Gets a value indicating whether the <see cref="T:System.Collections.IList" /> is read-only.
        /// </summary>
        bool IList.IsReadOnly
        {
            get { return IsReadOnly; }
        }

        /// <summary>
        ///     Gets a value indicating whether the <see cref="T:System.Collections.IList" /> has a fixed size.
        /// </summary>
        bool IList.IsFixedSize
        {
            get { return false; }
        }

        /// <summary>
        ///     Copies the elements of the <see cref="T:System.Collections.ICollection" /> to an <see cref="T:System.Array" />,
        ///     starting at a particular <see cref="T:System.Array" /> index.
        /// </summary>
        public void CopyTo(Array array, int index)
        {
            Array.Copy(Items, 0, array, index, Size);
        }

        /// <summary>
        ///     Gets the number of elements contained in the <see cref="T:System.Collections.ICollection" />.
        /// </summary>
        int ICollection.Count
        {
            get { return Count; }
        }

        /// <summary>
        ///     Gets an object that can be used to synchronize access to the <see cref="T:System.Collections.ICollection" />.
        /// </summary>
        object ICollection.SyncRoot
        {
            get
            {
                if (_syncRoot == null)
                    Interlocked.CompareExchange(ref _syncRoot, new object(), null);
                return _syncRoot;
            }
        }

        /// <summary>
        ///     Gets a value indicating whether access to the <see cref="T:System.Collections.ICollection" /> is synchronized
        ///     (thread safe).
        /// </summary>
        bool ICollection.IsSynchronized
        {
            get { return false; }
        }

        /// <summary>
        ///     Returns an enumerator that iterates through the collection.
        /// </summary>
        public IEnumerator<T> GetEnumerator()
        {
            return Items.Take(Size).GetEnumerator();
        }

        /// <summary>
        ///     Returns an enumerator that iterates through a collection.
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        ///     Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        void ICollection<T>.Add(T item)
        {
            Add(item);
        }

        /// <summary>
        ///     Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        public void Clear()
        {
            Array.Clear(Items, 0, Size);
            Size = 0;
        }

        /// <summary>
        ///     Determines whether the <see cref="T:System.Collections.Generic.ICollection`1" /> contains a specific value.
        /// </summary>
        public bool Contains(T item)
        {
            return IndexOfObject(item) != -1;
        }

        /// <summary>
        ///     Copies the elements of the <see cref="T:System.Collections.Generic.ICollection`1" /> to an
        ///     <see cref="T:System.Array" />, starting at a particular <see cref="T:System.Array" /> index.
        /// </summary>
        public void CopyTo(T[] array, int arrayIndex)
        {
            Array.Copy(Items, 0, array, arrayIndex, Size);
        }

        /// <summary>
        ///     Removes the first occurrence of a specific object from the
        ///     <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        public bool Remove(T item)
        {
            Should.NotBeNull(item, "item");
            int indexOf = IndexOfObject(item);
            if (indexOf == -1)
                return false;
            RemoveAt(indexOf);
            return true;
        }

        /// <summary>
        ///     Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        public int Count
        {
            get { return Size; }
        }

        /// <summary>
        ///     Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.
        /// </summary>
        public bool IsReadOnly
        {
            get { return false; }
        }

        /// <summary>
        ///     Determines the index of a specific item in the <see cref="T:System.Collections.Generic.IList`1" />.
        /// </summary>
        public int IndexOf(T item)
        {
            Should.NotBeNull(item, "item");
            return IndexOfObject(item);
        }

        /// <summary>
        ///     Inserts an item to the <see cref="T:System.Collections.Generic.IList`1" /> at the specified index.
        /// </summary>
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

        /// <summary>
        ///     Removes the <see cref="T:System.Collections.Generic.IList`1" /> item at the specified index.
        /// </summary>
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

        /// <summary>
        ///     Gets or sets the element at the specified index.
        /// </summary>
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

        /// <summary>
        ///     Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        public int Add(T item)
        {
            Should.NotBeNull(item, "item");
            return InsertInternal(GetInsertIndex(item), item);
        }

        #endregion
    }
}