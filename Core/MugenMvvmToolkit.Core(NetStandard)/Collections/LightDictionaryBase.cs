#region Copyright

// ****************************************************************************
// <copyright file="LightDictionaryBase.cs">
// Copyright (c) 2012-2016 Vyacheslav Volkov
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
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using MugenMvvmToolkit.Attributes;
using MugenMvvmToolkit.Infrastructure;

namespace MugenMvvmToolkit.Collections
{
    [DebuggerDisplay("Count = {Count}"), Serializable, DataContract(Namespace = ApplicationSettings.DataContractNamespace, IsReference = true)]
    public abstract class LightDictionaryBase<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
#if NET_STANDARD
        , IReadOnlyCollection<KeyValuePair<TKey, TValue>>
#endif
    {
        #region Nested types

        [StructLayout(LayoutKind.Auto), Serializable]
        internal struct Entry
        {
            public int HashCode;
            public TKey Key;
            public int Next;
            public TValue Value;
        }

        [StructLayout(LayoutKind.Auto)]
        public struct Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>
        {
            #region Fields

            private LightDictionaryBase<TKey, TValue> _dictionary;
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

            public KeyValuePair<TKey, TValue> Current => _current;
            
            object IEnumerator.Current => _current;

            public bool MoveNext()
            {
                for (; _index < _dictionary._count; ++_index)
                {
                    if (_dictionary._entries[_index].HashCode >= 0)
                    {
                        _current = new KeyValuePair<TKey, TValue>(_dictionary._entries[_index].Key,
                            _dictionary._entries[_index].Value);
                        ++_index;
                        return true;
                    }
                }
                _index = _dictionary._count + 1;
                _current = new KeyValuePair<TKey, TValue>();
                return false;
            }

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
        private int _count;

        [IgnoreDataMember, XmlIgnore]
        private Entry[] _entries;

        [IgnoreDataMember, NonSerialized, XmlIgnore]
        private int _freeCount;

        [IgnoreDataMember, NonSerialized, XmlIgnore]
        private int _freeList;

        #endregion

        #region Constructors

        protected LightDictionaryBase(bool initialize)
        {
            if (initialize)
                Initialize(0);
        }

        protected LightDictionaryBase(int capacity)
        {
            Initialize(capacity);
        }

        #endregion

        #region Properties

        [DataMember(Name = "vi"), Preserve]
        internal KeyValuePair<TKey, TValue>[] ValuesInternal
        {
            get { return ToArray(); }
            set { Restore(value); }
        }

        public int Count
        {
            get
            {
                if (_buckets == null)
                    RestoreState();
                return _count - _freeCount;
            }
        }

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
            for (int index = 0; index < _count; ++index)
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

        protected internal void Add(TKey key, TValue value)
        {
            Insert(key, value, true);
        }

        protected internal void Clear()
        {
            if (_buckets == null)
                RestoreState();
            if (_count <= 0)
                return;
            for (int index = 0; index < _buckets.Length; index++)
                _buckets[index] = -1;
            Array.Clear(_entries, 0, _count);
            _freeList = -1;
            _count = 0;
            _freeCount = 0;
        }

        protected internal bool ContainsKey(TKey key)
        {
            return FindEntry(key) >= 0;
        }

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

                    if (_entries.Length > 1000 && _entries.Length / 4 > Count)
                        TrimExcess();
                    return true;
                }
                index2 = index3;
            }
            return false;
        }

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

        public Enumerator GetEnumerator()
        {
            if (_buckets == null)
                RestoreState();
            return new Enumerator(this);
        }

        public KeyValuePair<TKey, TValue>[] ToArray()
        {
            if (_buckets == null)
                RestoreState();
            if (_count == 0)
                return Empty.Array<KeyValuePair<TKey, TValue>>();
            var result = new KeyValuePair<TKey, TValue>[Count];
            int index = 0;
            for (int i = 0; i < _count; i++)
            {
                Entry entry = _entries[i];
                if (entry.HashCode >= 0)
                    result[index++] = new KeyValuePair<TKey, TValue>(entry.Key, entry.Value);
            }
            return result;
        }

        protected abstract bool Equals(TKey x, TKey y);

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

        protected void Initialize(int capacity)
        {
            int prime = PrimeNumberHelper.GetPrime(capacity);
            _buckets = new int[prime];
            for (int index = 0; index < _buckets.Length; index++)
                _buckets[index] = -1;
            _entries = new Entry[prime];
            _freeList = -1;
            _count = 0;
        }

        private void Insert(TKey key, TValue value, bool add)
        {
            if (_buckets == null)
                RestoreState();
            int hashCode = GetHashCodeInternal(key);
            int index1 = hashCode % _buckets.Length;
            for (int index2 = _buckets[index1]; index2 >= 0; index2 = _entries[index2].Next)
            {
                if (_entries[index2].HashCode == hashCode && Equals(_entries[index2].Key, key))
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
                if (_count == _entries.Length)
                {
                    Resize();
                    index1 = hashCode % _buckets.Length;
                }
                index3 = _count;
                ++_count;
            }
            _entries[index3].HashCode = hashCode;
            _entries[index3].Next = _buckets[index1];
            _entries[index3].Key = key;
            _entries[index3].Value = value;
            _buckets[index1] = index3;
        }

        private void Resize()
        {
            Resize(PrimeNumberHelper.ExpandPrime(_count));
        }

        private void TrimExcess()
        {
            int count = _count - _freeCount;
            int newSize = PrimeNumberHelper.GetPrime(count);
            var numArray = new int[newSize];
            for (int i = 0; i < numArray.Length; i++)
                numArray[i] = -1;
            var entryArray = new Entry[newSize];
            int index = 0;
            for (int i = 0; i < _count; i++)
            {
                Entry entry = _entries[i];
                if (entry.HashCode >= 0)
                    entryArray[index++] = entry;
            }
            _freeList = -1;
            _freeCount = 0;
            _count = count;

            for (int i = 0; i < _count; i++)
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
            Array.Copy(_entries, 0, entryArray, 0, _count);
            for (int i = 0; i < _count; i++)
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
