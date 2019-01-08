using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using MugenMvvm.Attributes;
using MugenMvvm.Infrastructure;

namespace MugenMvvm.Collections
{
    [DebuggerDisplay("Count = {Count}")]
    [Serializable]
    [DataContract(Namespace = BuildConstants.DataContractNamespace, IsReference = true)]
    public abstract class LightDictionaryBase<TKey, TValue> : IReadOnlyCollection<KeyValuePair<TKey, TValue>>
    {
        #region Fields

        [IgnoreDataMember, XmlIgnore, NonSerialized]
        private int[] _buckets;

        [IgnoreDataMember, XmlIgnore, NonSerialized]
        private int _count;

        [IgnoreDataMember, XmlIgnore, NonSerialized]
        private Entry[] _entries;

        [IgnoreDataMember, XmlIgnore, NonSerialized]
        private int _freeCount;

        [IgnoreDataMember, XmlIgnore, NonSerialized]
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

        [DataMember(Name = "vi")]
        [Preserve(Conditional = true)]
        internal KeyValuePair<TKey, TValue>[] ValuesInternal
        {
            get => ToArray();
            set => Restore(value);
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
                var entry = FindEntry(key);
                if (entry >= 0)
                    return _entries[entry].Value;
                throw new KeyNotFoundException();
            }

            set => Insert(key, value, false);
        }

        #endregion

        #region Implementation of interfaces

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region Methods

        private void Restore(IList<KeyValuePair<TKey, TValue>> value)
        {
            Initialize(value.Count);
            for (var i = 0; i < value.Count; i++)
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
            for (var index = 0; index < _count; ++index)
            {
                if (_entries[index].HashCode >= 0)
                    oldValues.Add(new KeyValuePair<TKey, TValue>(_entries[index].Key, _entries[index].Value));
            }

            Restore(oldValues);
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
            for (var index = 0; index < _buckets.Length; index++)
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
            var num = GetHashCodeInternal(key);
            var index1 = num % _buckets.Length;
            var index2 = -1;
            for (var index3 = _buckets[index1]; index3 >= 0; index3 = _entries[index3].Next)
            {
                if (_entries[index3].HashCode == num && Equals(_entries[index3].Key, key))
                {
                    if (index2 < 0)
                        _buckets[index1] = _entries[index3].Next;
                    else
                        _entries[index2].Next = _entries[index3].Next;
                    _entries[index3].HashCode = -1;
                    _entries[index3].Next = _freeList;
                    _entries[index3].Key = default;
                    _entries[index3].Value = default;
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
            var entry = FindEntry(key);
            if (entry >= 0)
            {
                value = _entries[entry].Value;
                return true;
            }

            value = default;
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
                return Default.EmptyArray<KeyValuePair<TKey, TValue>>();
            var result = new KeyValuePair<TKey, TValue>[Count];
            var index = 0;
            for (var i = 0; i < _count; i++)
            {
                var entry = _entries[i];
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
            var num = GetHashCodeInternal(key);
            for (var index = _buckets[num % _buckets.Length]; index >= 0; index = _entries[index].Next)
            {
                if (_entries[index].HashCode == num && Equals(_entries[index].Key, key))
                    return index;
            }

            return -1;
        }

        protected void Initialize(int capacity)
        {
            var prime = PrimeNumberHelper.GetPrime(capacity);
            _buckets = new int[prime];
            for (var index = 0; index < _buckets.Length; index++)
                _buckets[index] = -1;
            _entries = new Entry[prime];
            _freeList = -1;
            _count = 0;
        }

        private void Insert(TKey key, TValue value, bool add)
        {
            if (_buckets == null)
                RestoreState();
            var hashCode = GetHashCodeInternal(key);
            var index1 = hashCode % _buckets.Length;
            for (var index2 = _buckets[index1]; index2 >= 0; index2 = _entries[index2].Next)
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
            var count = _count - _freeCount;
            var newSize = PrimeNumberHelper.GetPrime(count);
            var numArray = new int[newSize];
            for (var i = 0; i < numArray.Length; i++)
                numArray[i] = -1;
            var entryArray = new Entry[newSize];
            var index = 0;
            for (var i = 0; i < _count; i++)
            {
                var entry = _entries[i];
                if (entry.HashCode >= 0)
                    entryArray[index++] = entry;
            }

            _freeList = -1;
            _freeCount = 0;
            _count = count;

            for (var i = 0; i < _count; i++)
            {
                var index2 = entryArray[i].HashCode % newSize;
                entryArray[i].Next = numArray[index2];
                numArray[index2] = i;
            }

            _buckets = numArray;
            _entries = entryArray;
        }

        private void Resize(int newSize)
        {
            var numArray = new int[newSize];
            for (var index = 0; index < numArray.Length; index++)
                numArray[index] = -1;
            var entryArray = new Entry[newSize];
            Array.Copy(_entries, 0, entryArray, 0, _count);
            for (var i = 0; i < _count; i++)
            {
                var index2 = entryArray[i].HashCode % newSize;
                entryArray[i].Next = numArray[index2];
                numArray[index2] = i;
            }

            _buckets = numArray;
            _entries = entryArray;
        }

        #endregion

        #region Nested types

        [StructLayout(LayoutKind.Auto)]
        [Serializable]
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

            private readonly LightDictionaryBase<TKey, TValue> _dictionary;
            private int _index;

            #endregion

            #region Constructors

            internal Enumerator(LightDictionaryBase<TKey, TValue> dictionary)
            {
                _dictionary = dictionary;
                _index = 0;
                Current = new KeyValuePair<TKey, TValue>();
            }

            #endregion

            #region Properties

            public KeyValuePair<TKey, TValue> Current { get; private set; }

            object IEnumerator.Current => Current;

            #endregion

            #region Implementation of interfaces

            public bool MoveNext()
            {
                for (; _index < _dictionary._count; ++_index)
                {
                    if (_dictionary._entries[_index].HashCode >= 0)
                    {
                        Current = new KeyValuePair<TKey, TValue>(_dictionary._entries[_index].Key,
                            _dictionary._entries[_index].Value);
                        ++_index;
                        return true;
                    }
                }

                _index = _dictionary._count + 1;
                Current = new KeyValuePair<TKey, TValue>();
                return false;
            }

            public void Dispose()
            {
            }


            void IEnumerator.Reset()
            {
                _index = 0;
                Current = new KeyValuePair<TKey, TValue>();
            }

            #endregion
        }

        #endregion
    }
}