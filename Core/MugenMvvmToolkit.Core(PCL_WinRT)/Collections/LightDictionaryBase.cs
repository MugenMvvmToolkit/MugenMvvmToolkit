#region Copyright
// ****************************************************************************
// <copyright file="LightDictionaryBase.cs">
// Copyright © Vyacheslav Volkov 2012-2014
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
using System.Runtime.Serialization;
using System.Xml.Serialization;
using MugenMvvmToolkit.Infrastructure;

namespace MugenMvvmToolkit.Collections
{
    /// <summary>
    ///     Represents a collection of keys and values.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
    [DebuggerDisplay("Count = {Count}"), Serializable, DataContract(Namespace = ApplicationSettings.DataContractNamespace, IsReference = true)]
    public abstract class LightDictionaryBase<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
    {
        #region Nested types

        [Serializable]
        internal struct Entry
        {
            public int HashCode;

            public TKey Key;

            public int Next;

            public TValue Value;
        }

        /// <summary>
        ///     Enumerates the elements of a <see cref="LightDictionaryBase{TKey,TValue}" />.
        /// </summary>
        public struct Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>
        {
            #region Fields

            private readonly LightDictionaryBase<TKey, TValue> _dictionary;
            private KeyValuePair<TKey, TValue> _current;
            private int _index;

            #endregion

            #region Constructors

            internal Enumerator(LightDictionaryBase<TKey, TValue> dictionary)
            {
                _dictionary = dictionary;
                _index = 0;
                _current = new KeyValuePair<TKey, TValue>();
            }

            #endregion

            #region Implementation of IEnumerator<KeyValuePair<TKey, TValue>>

            /// <summary>
            ///     Gets the element at the current position of the enumerator.
            /// </summary>
            /// <returns>
            ///     The element in the <see cref="T:System.Collections.Generic.Dictionary`2" /> at the current position of the
            ///     enumerator.
            /// </returns>
            public KeyValuePair<TKey, TValue> Current
            {
                get { return _current; }
            }


            object IEnumerator.Current
            {
                get { return _current; }
            }

            /// <summary>
            ///     Advances the enumerator to the next element of the <see cref="T:System.Collections.Generic.Dictionary`2" />.
            /// </summary>
            /// <returns>
            ///     true if the enumerator was successfully advanced to the next element; false if the enumerator has passed the end of
            ///     the collection.
            /// </returns>
            /// <exception cref="T:System.InvalidOperationException">The collection was modified after the enumerator was created. </exception>
            public bool MoveNext()
            {
                for (; _index < _dictionary._countInternal; ++_index)
                {
                    if (_dictionary._entries[_index].HashCode >= 0)
                    {
                        _current = new KeyValuePair<TKey, TValue>(_dictionary._entries[_index].Key,
                            _dictionary._entries[_index].Value);
                        ++_index;
                        return true;
                    }
                }
                _index = _dictionary._countInternal + 1;
                _current = new KeyValuePair<TKey, TValue>();
                return false;
            }

            /// <summary>
            ///     Releases all resources used by the <see cref="T:System.Collections.Generic.Dictionary`2.Enumerator" />.
            /// </summary>
            public void Dispose()
            {
            }


            void IEnumerator.Reset()
            {
                _index = 0;
                _current = new KeyValuePair<TKey, TValue>();
            }

            #endregion
        }

        #endregion

        #region Fields

        [IgnoreDataMember, NonSerialized, XmlIgnore]
        private int[] _buckets;

        [IgnoreDataMember, XmlIgnore]
        private int _countInternal;

        [IgnoreDataMember, XmlIgnore]
        private Entry[] _entries;

        [IgnoreDataMember, NonSerialized, XmlIgnore]
        private int _freeCount;

        [IgnoreDataMember, NonSerialized, XmlIgnore]
        private int _freeList;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="LightDictionaryBase{TKey,TValue}" /> class.
        /// </summary>
        protected LightDictionaryBase(bool initialize)
        {
            if (initialize)
                Initialize(0);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="LightDictionaryBase{TKey,TValue}" /> class.
        /// </summary>
        protected LightDictionaryBase(int capacity)
        {
            Initialize(capacity);
        }

        #endregion

        #region Properties

        [DataMember]
        internal KeyValuePair<TKey, TValue>[] ValuesInternal
        {
            get { return ToArray(); }
            set { Restore(value); }
        }

        /// <summary>
        ///     Gets the number of key/value pairs contained in the <see cref="T:System.Collections.Generic.Dictionary`2" />.
        /// </summary>
        /// <returns>
        ///     The number of key/value pairs contained in the <see cref="T:System.Collections.Generic.Dictionary`2" />.
        /// </returns>
        public int Count
        {
            get
            {
                if (_buckets == null)
                    RestoreState();
                return _countInternal - _freeCount;
            }
        }

        /// <summary>
        ///     Gets or sets the value associated with the specified key.
        /// </summary>
        /// <returns>
        ///     The value associated with the specified key. If the specified key is not found, a get operation throws a
        ///     <see cref="T:System.Collections.Generic.KeyNotFoundException" />, and a set operation creates a new element with
        ///     the specified key.
        /// </returns>
        /// <param name="key">The key of the value to get or set.</param>
        protected internal TValue this[TKey key]
        {
            get
            {
                int entry = FindEntry(key);
                if (entry >= 0)
                    return _entries[entry].Value;
                throw new KeyNotFoundException();
            }

            set { Insert(key, value, false); }
        }

        #endregion

        #region Methods

        private void Restore(IList<KeyValuePair<TKey, TValue>> value)
        {
            Initialize(value.Count);
            for (int i = 0; i < value.Count; i++)
            {
                var pair = value[i];
                Add(pair.Key, pair.Value);
            }
        }

        private void RestoreState()
        {
            if (_buckets != null)
                return;
            var oldValues = new List<KeyValuePair<TKey, TValue>>();
            for (int index = 0; index < _countInternal; ++index)
            {
                if (_entries[index].HashCode >= 0)
                    oldValues.Add(new KeyValuePair<TKey, TValue>(_entries[index].Key, _entries[index].Value));
            }
            Restore(oldValues);
        }

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        ///     Adds the specified key and value to the dictionary.
        /// </summary>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="value">The value of the element to add. The value can be null for reference types.</param>
        protected internal void Add(TKey key, TValue value)
        {
            Insert(key, value, true);
        }

        /// <summary>
        ///     Removes all keys and values from the <see cref="T:System.Collections.Generic.Dictionary`2" />.
        /// </summary>
        protected internal void Clear()
        {
            if (_buckets == null)
                RestoreState();
            if (_countInternal <= 0)
                return;
            for (int index = 0; index < _buckets.Length; index++)
                _buckets[index] = -1;
            Array.Clear(_entries, 0, _countInternal);
            _freeList = -1;
            _countInternal = 0;
            _freeCount = 0;
        }

        /// <summary>
        ///     Determines whether the <see cref="T:System.Collections.Generic.Dictionary`2" /> contains the specified key.
        /// </summary>
        /// <returns>
        ///     true if the <see cref="T:System.Collections.Generic.Dictionary`2" /> contains an element with the specified key;
        ///     otherwise, false.
        /// </returns>
        /// <param name="key">The key to locate in the <see cref="T:System.Collections.Generic.Dictionary`2" />.</param>
        protected internal bool ContainsKey(TKey key)
        {
            return FindEntry(key) >= 0;
        }

        /// <summary>
        ///     Removes the value with the specified key from the <see cref="T:System.Collections.Generic.Dictionary`2" />.
        /// </summary>
        /// <returns>
        ///     true if the element is successfully found and removed; otherwise, false.  This method returns false if
        ///     <paramref name="key" /> is not found in the <see cref="T:System.Collections.Generic.Dictionary`2" />.
        /// </returns>
        /// <param name="key">The key of the element to remove.</param>
        protected internal bool Remove(TKey key)
        {
            if (_buckets == null)
                RestoreState();
            int num = GetHashCodeInternal(key);
            int index1 = num % _buckets.Length;
            int index2 = -1;
            for (int index3 = _buckets[index1]; index3 >= 0; index3 = _entries[index3].Next)
            {
                if (_entries[index3].HashCode == num && Equals(_entries[index3].Key, key))
                {
                    if (index2 < 0)
                        _buckets[index1] = _entries[index3].Next;
                    else
                        _entries[index2].Next = _entries[index3].Next;
                    _entries[index3].HashCode = -1;
                    _entries[index3].Next = _freeList;
                    _entries[index3].Key = default(TKey);
                    _entries[index3].Value = default(TValue);
                    _freeList = index3;
                    ++_freeCount;

                    if (_entries.Length > 1000 && (_entries.Length / 4) > Count)
                        TrimExcess();
                    return true;
                }
                index2 = index3;
            }
            return false;
        }

        /// <summary>
        ///     Gets the value associated with the specified key.
        /// </summary>
        /// <returns>
        ///     true if the <see cref="T:System.Collections.Generic.Dictionary`2" /> contains an element with the specified key;
        ///     otherwise, false.
        /// </returns>
        /// <param name="key">The key of the value to get.</param>
        /// <param name="value">
        ///     When this method returns, contains the value associated with the specified key, if the key is
        ///     found; otherwise, the default value for the type of the <paramref name="value" /> parameter. This parameter is
        ///     passed uninitialized.
        /// </param>
        protected internal bool TryGetValue(TKey key, out TValue value)
        {
            int entry = FindEntry(key);
            if (entry >= 0)
            {
                value = _entries[entry].Value;
                return true;
            }
            value = default(TValue);
            return false;
        }

        /// <summary>
        ///     Returns an enumerator that iterates through the <see cref="T:System.Collections.Generic.Dictionary`2" />.
        /// </summary>
        /// <returns>
        ///     A <see cref="T:System.Collections.Generic.Dictionary`2.Enumerator" /> structure for the
        ///     <see cref="T:System.Collections.Generic.Dictionary`2" />.
        /// </returns>
        public Enumerator GetEnumerator()
        {
            if (_buckets == null)
                RestoreState();
            return new Enumerator(this);
        }

        /// <summary>
        ///     Copies the elements of the <see cref="LightDictionaryBase{TKey,TValue}" /> to a new array.
        /// </summary>
        /// <returns>
        ///     An array containing copies of the elements of the <see cref="LightDictionaryBase{TKey,TValue}" />.
        /// </returns>
        public KeyValuePair<TKey, TValue>[] ToArray()
        {
            if (_buckets == null)
                RestoreState();
            if (_countInternal == 0)
                return Empty.Array<KeyValuePair<TKey, TValue>>();
            var result = new KeyValuePair<TKey, TValue>[Count];
            int index = 0;
            for (int i = 0; i < _countInternal; i++)
            {
                Entry entry = _entries[i];
                if (entry.HashCode >= 0)
                    result[index++] = new KeyValuePair<TKey, TValue>(entry.Key, entry.Value);
            }
            return result;
        }

        /// <summary>
        ///     Determines whether the specified objects are equal.
        /// </summary>
        protected abstract bool Equals(TKey x, TKey y);

        /// <summary>
        ///     Returns a hash code for the specified object.
        /// </summary>
        protected abstract int GetHashCode(TKey key);

        private int GetHashCodeInternal(TKey key)
        {
            return GetHashCode(key) & int.MaxValue;
        }

        private int FindEntry(TKey key)
        {
            if (_buckets == null)
                RestoreState();
            int num = GetHashCodeInternal(key);
            for (int index = _buckets[num % _buckets.Length]; index >= 0; index = _entries[index].Next)
            {
                if (_entries[index].HashCode == num && Equals(_entries[index].Key, key))
                    return index;
            }
            return -1;
        }

        /// <summary>
        ///     Initialize the capacity.
        /// </summary>
        protected void Initialize(int capacity)
        {
            int prime = PrimeNumberHelper.GetPrime(capacity);
            _buckets = new int[prime];
            for (int index = 0; index < _buckets.Length; index++)
                _buckets[index] = -1;
            _entries = new Entry[prime];
            _freeList = -1;
            _countInternal = 0;
        }

        private void Insert(TKey key, TValue value, bool add)
        {
            if (_buckets == null)
                RestoreState();
            int num1 = GetHashCodeInternal(key);
            int index1 = num1 % _buckets.Length;
            for (int index2 = _buckets[index1]; index2 >= 0; index2 = _entries[index2].Next)
            {
                if (_entries[index2].HashCode == num1 && Equals(_entries[index2].Key, key))
                {
                    if (add)
                        throw new ArgumentException("An item with the same key has already been added.");
                    _entries[index2].Value = value;
                    return;
                }
            }
            int index3;
            if (_freeCount > 0)
            {
                index3 = _freeList;
                _freeList = _entries[index3].Next;
                --_freeCount;
            }
            else
            {
                if (_countInternal == _entries.Length)
                {
                    Resize();
                    index1 = num1 % _buckets.Length;
                }
                index3 = _countInternal;
                ++_countInternal;
            }
            _entries[index3].HashCode = num1;
            _entries[index3].Next = _buckets[index1];
            _entries[index3].Key = key;
            _entries[index3].Value = value;
            _buckets[index1] = index3;
        }

        private void Resize()
        {
            Resize(PrimeNumberHelper.ExpandPrime(_countInternal));
        }

        private void TrimExcess()
        {
            int realCount = _countInternal - _freeCount;
            int newSize = PrimeNumberHelper.GetPrime(realCount);
            var numArray = new int[newSize];
            for (int i = 0; i < numArray.Length; i++)
                numArray[i] = -1;
            var entryArray = new Entry[newSize];
            int index = 0;
            for (int i = 0; i < _countInternal; i++)
            {
                Entry entry = _entries[i];
                if (entry.HashCode >= 0)
                    entryArray[index++] = entry;
            }
            _freeList = -1;
            _freeCount = 0;
            _countInternal = realCount;

            for (int i = 0; i < _countInternal; i++)
            {
                int index2 = entryArray[i].HashCode % newSize;
                entryArray[i].Next = numArray[index2];
                numArray[index2] = i;
            }
            _buckets = numArray;
            _entries = entryArray;
        }

        private void Resize(int newSize)
        {
            var numArray = new int[newSize];
            for (int index = 0; index < numArray.Length; index++)
                numArray[index] = -1;
            var entryArray = new Entry[newSize];
            Array.Copy(_entries, 0, entryArray, 0, _countInternal);
            for (int i = 0; i < _countInternal; i++)
            {
                int index2 = entryArray[i].HashCode % newSize;
                entryArray[i].Next = numArray[index2];
                numArray[index2] = i;
            }
            _buckets = numArray;
            _entries = entryArray;
        }

        #endregion
    }
}