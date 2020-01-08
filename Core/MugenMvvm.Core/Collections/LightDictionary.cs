using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using MugenMvvm.Attributes;
using MugenMvvm.Constants;
using MugenMvvm.Extensions;
using MugenMvvm.Internal;

namespace MugenMvvm.Collections
{
    [DebuggerDisplay("Count = {Count}")]
    [Serializable]
    [DataContract(Namespace = BuildConstant.DataContractNamespace)]
    public class LightDictionary<TKey, TValue> : IReadOnlyDictionary<TKey, TValue> where TKey : notnull
    {
        #region Fields

        [IgnoreDataMember]
        [XmlIgnore]
        [NonSerialized]
        private int[] _buckets;

        [IgnoreDataMember]
        [XmlIgnore]
        [NonSerialized]
        private int _count;

        [IgnoreDataMember]
        [XmlIgnore]
        [NonSerialized]
        private Entry[] _entries;

        [IgnoreDataMember]
        [XmlIgnore]
        [NonSerialized]
        private int _freeCount;

        [IgnoreDataMember]
        [XmlIgnore]
        [NonSerialized]
        private int _freeList;

        private static readonly IEqualityComparer<TKey> DefaultComparer = EqualityComparer<TKey>.Default;

        #endregion

        #region Constructors

#pragma warning disable CS8618
        public LightDictionary()
            : this(3)
        {
        }

        public LightDictionary(int capacity)
        {
            Initialize(capacity);
        }
#pragma warning restore CS8618

        #endregion

        #region Properties

        [DataMember(Name = "vi")]
        [Preserve(Conditional = true)]
        internal KeyValuePair<TKey, TValue>[] ValuesInternal
        {
            get => ToArray();
            set
            {
                Initialize(value.Length);
                for (var i = 0; i < value.Length; i++)
                {
                    var pair = value[i];
                    Add(pair.Key, pair.Value);
                }
            }
        }

        public int Count => _count - _freeCount;

        public TValue this[TKey key]
        {
            get
            {
                var hashCode = GetHashCode(key) & int.MaxValue;
                for (var i = _buckets![hashCode % _buckets.Length]; i >= 0; i = _entries[i].Next)
                {
                    if (_entries[i].HashCode == hashCode && Equals(_entries[i].Key, key))
                        return _entries[i].Value;
                }

                ExceptionManager.ThrowKeyNotFound();
                return default;
            }

            set => Insert(key, value, false);
        }

        public IEnumerable<TKey> Keys => KeysToArray();

        public IEnumerable<TValue> Values => ValuesToArray();

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

        public bool ContainsKey(TKey key)
        {
            var hashCode = GetHashCode(key) & int.MaxValue;
            for (var i = _buckets![hashCode % _buckets.Length]; i >= 0; i = _entries[i].Next)
            {
                if (_entries[i].HashCode == hashCode && Equals(_entries[i].Key, key))
                    return true;
            }

            return false;
        }

        public bool TryGetValue(TKey key, [NotNullWhen(true)] out TValue value)
        {
            var hashCode = GetHashCode(key) & int.MaxValue;
            for (var i = _buckets![hashCode % _buckets.Length]; i >= 0; i = _entries[i].Next)
            {
                if (_entries[i].HashCode == hashCode && Equals(_entries[i].Key, key))
                {
                    value = _entries[i].Value;
                    return true;
                }
            }

            value = default!;
            return false;
        }

        #endregion

        #region Methods

        public void Add(TKey key, TValue value)
        {
            Insert(key, value, true);
        }

        public void Clear()
        {
            if (_count <= 0)
                return;
            for (var index = 0; index < _buckets!.Length; index++)
                _buckets[index] = -1;
            Array.Clear(_entries, 0, _count);
            _freeList = -1;
            _count = 0;
            _freeCount = 0;
        }

        public bool Remove(TKey key)
        {
            var hashCode = GetHashCode(key) & int.MaxValue;
            var bucket = hashCode % _buckets!.Length;
            var last = -1;
            for (var i = _buckets[bucket]; i >= 0; i = _entries[i].Next)
            {
                if (_entries[i].HashCode == hashCode && Equals(_entries[i].Key, key))
                {
                    if (last < 0)
                        _buckets[bucket] = _entries[i].Next;
                    else
                        _entries[last].Next = _entries[i].Next;
                    _entries[i].HashCode = -1;
                    _entries[i].Next = _freeList;
                    _entries[i].Key = default!;
                    _entries[i].Value = default!;
                    _freeList = i;
                    ++_freeCount;

                    if (_entries.Length > 1000 && _entries.Length / 4 > Count)
                        TrimExcess();
                    return true;
                }

                last = i;
            }

            return false;
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        public KeyValuePair<TKey, TValue>[] ToArray()
        {
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

        public TKey[] KeysToArray()
        {
            var count = Count;
            if (count == 0)
                return Default.EmptyArray<TKey>();
            var result = new TKey[count];
            var index = 0;
            for (var i = 0; i < _count; i++)
            {
                var entry = _entries[i];
                if (entry.HashCode >= 0)
                    result[index++] = entry.Key;
            }

            return result;
        }

        public TValue[] ValuesToArray()
        {
            var count = Count;
            if (count == 0)
                return Default.EmptyArray<TValue>();
            var result = new TValue[count];
            var index = 0;
            for (var i = 0; i < _count; i++)
            {
                var entry = _entries[i];
                if (entry.HashCode >= 0)
                    result[index++] = entry.Value;
            }

            return result;
        }

        public void Clone(LightDictionary<TKey, TValue> clone, Func<TValue, TValue>? valueConverter = null)
        {
            clone._buckets = _buckets.ToArray();
            clone._count = _count;
            if (_entries != null)
            {
                if (valueConverter == null)
                    clone._entries = _entries.ToArray();
                else
                {
                    var entries = new Entry[_entries.Length];
                    for (var i = 0; i < entries.Length; i++)
                    {
                        var old = _entries[i];
                        entries[i] = new Entry
                        {
                            HashCode = old.HashCode,
                            Next = old.Next,
                            Key = old.Key,
                            Value = valueConverter(old.Value)
                        };
                    }

                    clone._entries = entries;
                }
            }

            clone._freeCount = _freeCount;
            clone._freeList = _freeList;
        }

        protected virtual bool Equals(TKey x, TKey y)
        {
            return DefaultComparer.Equals(x, y);
        }

        protected virtual int GetHashCode(TKey key)
        {
            return DefaultComparer.GetHashCode(key);
        }

        protected void Initialize(int capacity)
        {
            if (capacity == 0)
            {
                _buckets = Default.EmptyArray<int>();
                _entries = Default.EmptyArray<Entry>();
                return;
            }

            var prime = PrimeNumberHelper.GetPrime(capacity);
            _buckets = new int[prime];
            for (var i = 0; i < _buckets.Length; i++)
                _buckets[i] = -1;
            _entries = new Entry[prime];
            _freeList = -1;
            _count = 0;
        }

        private void Insert(TKey key, TValue value, bool add)
        {
            var hashCode = GetHashCode(key) & int.MaxValue;
            var targetBucket = hashCode % _buckets!.Length;
            for (var i = _buckets[targetBucket]; i >= 0; i = _entries[i].Next)
            {
                if (_entries[i].HashCode == hashCode && Equals(_entries[i].Key, key))
                {
                    if (add)
                        ExceptionManager.ThrowDuplicateKey();
                    _entries[i].Value = value;
                    return;
                }
            }

            int index;
            if (_freeCount > 0)
            {
                index = _freeList;
                _freeList = _entries[index].Next;
                --_freeCount;
            }
            else
            {
                if (_count == _entries.Length)
                {
                    Resize();
                    targetBucket = hashCode % _buckets.Length;
                }

                index = _count;
                ++_count;
            }

            _entries[index].HashCode = hashCode;
            _entries[index].Next = _buckets[targetBucket];
            _entries[index].Key = key;
            _entries[index].Value = value;
            _buckets[targetBucket] = index;
        }

        private void Resize()
        {
            Resize(PrimeNumberHelper.ExpandPrime(_count));
        }

        private void TrimExcess()
        {
            var count = _count - _freeCount;
            var newSize = PrimeNumberHelper.GetPrime(count);
            if (_count == newSize)
                return;
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

            // ReSharper disable once FieldCanBeMadeReadOnly.Local
            private LightDictionary<TKey, TValue> _dictionary;
            private int _index;

            #endregion

            #region Constructors

            internal Enumerator(LightDictionary<TKey, TValue> dictionary)
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