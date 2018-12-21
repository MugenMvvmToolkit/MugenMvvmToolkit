using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using MugenMvvm.Interfaces;
using MugenMvvm.Interfaces.Serialization;
using MugenMvvm.Models;
using MugenMvvm.Attributes;

namespace MugenMvvm.Infrastructure
{
    public class Context : IContext, IEqualityComparer<IContextKey>
    {
        #region Fields

        private readonly Dictionary<IContextKey, object?> _values;

        #endregion

        #region Constructors

        public Context()
        {
            _values = new Dictionary<IContextKey, object?>(this);
        }

        private Context(Dictionary<IContextKey, object?> values)
        {
            _values = values;
        }

        #endregion

        #region Properties

        public int Count
        {
            get
            {
                lock (_values)
                {
                    return _values.Count;
                }
            }
        }

        #endregion

        #region Implementation of interfaces

        public IEnumerator<ContextValue> GetEnumerator()
        {
            IEnumerable<ContextValue> list;
            lock (_values)
            {
                list = _values.ToArray(pair => new ContextValue(pair.Key, pair.Value));
            }

            return list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IMemento? GetMemento()
        {
            return new ContextMemento(this);
        }

        public bool TryGet(IContextKey key, out object? value)
        {
            Should.NotBeNull(key, nameof(key));
            object? obj;
            bool hasValue;
            lock (_values)
            {
                hasValue = _values.TryGetValue(key, out obj);
            }

            if (hasValue)
            {
                value = obj;
                return true;
            }

            value = default;
            return false;
        }

        public bool TryGet<T>(IContextKey<T> key, out T value)
        {
            if (TryGet(key, out object? objValue) && objValue is T v)
            {
                value = v;
                return true;
            }
            value = default!;
            return false;
        }

        public bool Contains(IContextKey key)
        {
            Should.NotBeNull(key, nameof(key));
            lock (_values)
            {
                return _values.ContainsKey(key);
            }
        }

        public void Set(IContextKey key, object? value)
        {
            Should.NotBeNull(key, nameof(key));
            key.Validate(value);
            lock (_values)
            {
                _values[key] = value;
            }
        }

        public void Set<T>(IContextKey<T> key, T value)
        {
            object? obj = value;
            Set(key, obj);
        }

        public void Merge(IEnumerable<ContextValue> items)
        {
            Should.NotBeNull(items, nameof(items));
            lock (_values)
            {
                foreach (var item in items)
                    _values[item.Key] = item.Value;
            }
        }

        public bool Remove(IContextKey key)
        {
            Should.NotBeNull(key, nameof(key));
            lock (_values)
            {
                return _values.Remove(key);
            }
        }

        public void Clear()
        {
            lock (_values)
            {
                _values.Clear();
            }
        }

        bool IEqualityComparer<IContextKey>.Equals(IContextKey x, IContextKey y)
        {
            return string.Equals(x.Key, y.Key, StringComparison.Ordinal);
        }

        int IEqualityComparer<IContextKey>.GetHashCode(IContextKey obj)
        {
            if (obj == null)
                return 0;
            return obj.GetHashCode();
        }

        #endregion

        #region Methods

        public void Add<T>(IContextKey<T> constant, T value)
        {
            Set(constant, value);
        }

        #endregion

        #region Nested types

        [Serializable]
        [DataContract(Namespace = BuildConstants.DataContractNamespace)]
        [Preserve(Conditional = true, AllMembers = true)]
        public sealed class ContextMemento : IMemento
        {
            #region Fields

            [IgnoreDataMember]
            [XmlIgnore]
            private Context? _context;

            [DataMember(Name = "K")]
            internal IList<IContextKey>? Keys;

            [DataMember(Name = "V")]
            internal IList<object?>? Values;

            #endregion

            #region Constructors

            internal ContextMemento()
            {
            }

            internal ContextMemento(Context context)
            {
                _context = context;
            }

            #endregion

            #region Properties

            [IgnoreDataMember]
            public Type TargetType => typeof(Context);

            #endregion

            #region Implementation of interfaces

            public void Preserve(ISerializationContext serializationContext)
            {
                if (_context == null)
                    return;
                KeyValuePair<IContextKey, object?>[] currentValues;
                lock (_context._values)
                {
                    currentValues = _context._values.ToArray();
                }

                Keys = new List<IContextKey>();
                Values = new List<object?>();
                foreach (var keyPair in currentValues)
                {
                    if (!keyPair.Key.CanSerialize(keyPair.Value, serializationContext))
                        continue;

                    Keys.Add(keyPair.Key);
                    Values.Add(Default.SerializableNullValue.To(keyPair.Value));
                }
            }

            public IMementoResult Restore(ISerializationContext serializationContext)
            {
                Should.NotBeNull(Keys, nameof(Keys));
                Should.NotBeNull(Values, nameof(Values));
                if (serializationContext.Mode != SerializationMode.Clone && _context != null)
                    return new MementoResult(_context, serializationContext.Metadata);

                lock (this)
                {
                    if (serializationContext.Mode != SerializationMode.Clone && _context != null)
                        return new MementoResult(_context, serializationContext.Metadata);

                    var dictionary = new Dictionary<IContextKey, object?>();
                    for (var i = 0; i < Keys!.Count; i++)
                    {
                        var key = Keys![i];
                        var value = Values![i];
                        if (key != null && value != null)
                            dictionary[key] = Default.SerializableNullValue.From(value);
                    }

                    _context = new Context(dictionary);
                    return new MementoResult(_context, serializationContext.Metadata);
                }
            }

            #endregion
        }

        #endregion
    }
}