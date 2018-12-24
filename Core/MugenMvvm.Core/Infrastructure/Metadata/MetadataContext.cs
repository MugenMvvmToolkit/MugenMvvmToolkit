using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using MugenMvvm.Attributes;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Serialization;
using MugenMvvm.Models;

namespace MugenMvvm.Infrastructure.Metadata
{
    public class MetadataContext : IMetadataContext, IEqualityComparer<IMetadataContextKey>
    {
        #region Fields

        private readonly Dictionary<IMetadataContextKey, object?> _values;

        #endregion

        #region Constructors

        public MetadataContext()
        {
            _values = new Dictionary<IMetadataContextKey, object?>(this);
        }

        private MetadataContext(Dictionary<IMetadataContextKey, object?> values)
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

        public bool TryGet(IMetadataContextKey contextKey, out object? value)
        {
            Should.NotBeNull(contextKey, nameof(contextKey));
            object? obj;
            bool hasValue;
            lock (_values)
            {
                hasValue = _values.TryGetValue(contextKey, out obj);
            }

            if (hasValue)
            {
                value = obj;
                return true;
            }

            value = default;
            return false;
        }

        public bool TryGet<T>(IMetadataContextKey<T> contextKey, out T value)
        {
            if (TryGet(contextKey, out object? objValue) && objValue is T v)
            {
                value = v;
                return true;
            }
            value = default!;
            return false;
        }

        public bool Contains(IMetadataContextKey contextKey)
        {
            Should.NotBeNull(contextKey, nameof(contextKey));
            lock (_values)
            {
                return _values.ContainsKey(contextKey);
            }
        }

        public void Set(IMetadataContextKey contextKey, object? value)
        {
            Should.NotBeNull(contextKey, nameof(contextKey));
            contextKey.Validate(value);
            lock (_values)
            {
                _values[contextKey] = value;
            }
        }

        public void Set<T>(IMetadataContextKey<T> contextKey, T value)
        {
            object? obj = value;
            Set(contextKey, obj);
        }

        public void Merge(IEnumerable<ContextValue> items)
        {
            Should.NotBeNull(items, nameof(items));
            lock (_values)
            {
                foreach (var item in items)
                    _values[item.ContextKey] = item.Value;
            }
        }

        public bool Remove(IMetadataContextKey contextKey)
        {
            Should.NotBeNull(contextKey, nameof(contextKey));
            lock (_values)
            {
                return _values.Remove(contextKey);
            }
        }

        public void Clear()
        {
            lock (_values)
            {
                _values.Clear();
            }
        }

        bool IEqualityComparer<IMetadataContextKey>.Equals(IMetadataContextKey x, IMetadataContextKey y)
        {
            return string.Equals(x.Key, y.Key, StringComparison.Ordinal);
        }

        int IEqualityComparer<IMetadataContextKey>.GetHashCode(IMetadataContextKey obj)
        {
            if (obj == null)
                return 0;
            return obj.GetHashCode();
        }

        #endregion

        #region Methods

        public void Add<T>(IMetadataContextKey<T> constant, T value)
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
            private MetadataContext? _metadataContext;

            [DataMember(Name = "K")]
            internal IList<IMetadataContextKey>? Keys;

            [DataMember(Name = "V")]
            internal IList<object?>? Values;

            #endregion

            #region Constructors

            internal ContextMemento()
            {
            }

            internal ContextMemento(MetadataContext metadataContext)
            {
                _metadataContext = metadataContext;
            }

            #endregion

            #region Properties

            [IgnoreDataMember]
            public Type TargetType => typeof(MetadataContext);

            #endregion

            #region Implementation of interfaces

            public void Preserve(ISerializationContext serializationContext)
            {
                if (_metadataContext == null)
                    return;
                KeyValuePair<IMetadataContextKey, object?>[] currentValues;
                lock (_metadataContext._values)
                {
                    currentValues = _metadataContext._values.ToArray();
                }

                Keys = new List<IMetadataContextKey>();
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
                if (serializationContext.Mode != SerializationMode.Clone && _metadataContext != null)
                    return new MementoResult(_metadataContext, serializationContext.Metadata);

                lock (this)
                {
                    if (serializationContext.Mode != SerializationMode.Clone && _metadataContext != null)
                        return new MementoResult(_metadataContext, serializationContext.Metadata);

                    var dictionary = new Dictionary<IMetadataContextKey, object?>();
                    for (var i = 0; i < Keys!.Count; i++)
                    {
                        var key = Keys![i];
                        var value = Values![i];
                        if (key != null && value != null)
                            dictionary[key] = Default.SerializableNullValue.From(value);
                    }

                    _metadataContext = new MetadataContext(dictionary);
                    return new MementoResult(_metadataContext, serializationContext.Metadata);
                }
            }

            #endregion
        }

        #endregion
    }
}