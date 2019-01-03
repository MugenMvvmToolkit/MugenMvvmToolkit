using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using MugenMvvm.Attributes;
using MugenMvvm.Infrastructure.Internal;
using MugenMvvm.Infrastructure.Serialization;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Serialization;

namespace MugenMvvm.Infrastructure.Metadata
{
    public class MetadataContext : HasListenersBase<IObservableMetadataContextListener>, IObservableMetadataContext
    {
        #region Fields

        private readonly Dictionary<IMetadataContextKey, object?> _values;
        private readonly IObservableMetadataContextListener? _internalListener;

        #endregion

        #region Constructors

        public MetadataContext(IObservableMetadataContextListener? internalListener = null)
        {
            _values = new Dictionary<IMetadataContextKey, object?>();
            _internalListener = internalListener;
        }

        public MetadataContext(IReadOnlyMetadataContext metadataContext, IObservableMetadataContextListener? internalListener = null)
            : this(internalListener)
        {
            Should.NotBeNull(metadataContext, nameof(metadataContext));
            Merge(metadataContext);
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

        private bool HasListeners => _internalListener != null || Listeners != null;

        #endregion

        #region Implementation of interfaces

        public IEnumerator<MetadataContextValue> GetEnumerator()
        {
            IEnumerable<MetadataContextValue> list;
            lock (_values)
            {
                list = _values.ToArray(pair => new MetadataContextValue(pair));
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

        public bool TryGet<T>(IMetadataContextKey<T> contextKey, out T value, T defaultValue = default)
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
                value = contextKey.GetValue(this, obj);
                return true;
            }

            value = contextKey.GetDefaultValue(this, defaultValue);
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

        public T GetOrAdd<T>(IMetadataContextKey<T> contextKey, Func<IMetadataContext, object?, object?, T> valueFactory, object? state1, object? state2)
        {
            Should.NotBeNull(contextKey, nameof(contextKey));
            Should.NotBeNull(valueFactory, nameof(valueFactory));
            object? value;
            bool added = false;
            lock (_values)
            {
                if (!_values.TryGetValue(contextKey, out value))
                {
                    added = true;
                    value = contextKey.SetValue(this, valueFactory(this, state1, state2));
                    _values[contextKey] = value;
                }
            }

            if (added)
                OnAdded(contextKey, value);
            return contextKey.GetValue(this, value);
        }

        public void Set<T>(IMetadataContextKey<T> contextKey, T value)
        {
            Should.NotBeNull(contextKey, nameof(contextKey));
            contextKey.Validate(value);
            object? obj = contextKey.SetValue(this, value);
            if (HasListeners)
            {
                object? oldValue;
                bool hasOldValue;
                lock (_values)
                {
                    hasOldValue = _values.TryGetValue(contextKey, out oldValue);
                    _values[contextKey] = obj;
                }

                if (hasOldValue)
                    OnChanged(contextKey, oldValue, obj);
                else
                    OnAdded(contextKey, obj);
            }
            else
            {
                lock (_values)
                {
                    _values[contextKey] = obj;
                }
            }
        }

        public void Merge(IEnumerable<MetadataContextValue> items)
        {
            Should.NotBeNull(items, nameof(items));
            if (HasListeners)
            {
                var values = new List<KeyValuePair<MetadataContextValue, object?>>();
                lock (_values)
                {
                    foreach (var item in items)
                    {
                        KeyValuePair<MetadataContextValue, object?> value;
                        if (_values.TryGetValue(item.ContextKey, out var oldValue))
                            value = new KeyValuePair<MetadataContextValue, object?>(item, oldValue);
                        else
                            value = new KeyValuePair<MetadataContextValue, object?>(item, _values);
                        values.Add(value);
                        _values[item.ContextKey] = item.Value;
                    }
                }

                foreach (var pair in values)
                {
                    if (ReferenceEquals(pair.Value, _values))
                        OnAdded(pair.Key.ContextKey, pair.Key.Value);
                    else
                        OnChanged(pair.Key.ContextKey, pair.Value, pair.Key.Value);
                }
            }
            else
            {
                lock (_values)
                {
                    foreach (var item in items)
                    {
                        _values[item.ContextKey] = item.Value;
                    }
                }
            }
        }

        public bool Remove(IMetadataContextKey contextKey)
        {
            Should.NotBeNull(contextKey, nameof(contextKey));
            if (HasListeners)
            {
                bool changed;
                object? oldValue;
                lock (_values)
                {
                    changed = _values.TryGetValue(contextKey, out oldValue) && _values.Remove(contextKey);
                }

                if (changed)
                    OnRemoved(contextKey, oldValue);
                return changed;
            }

            lock (_values)
            {
                return _values.Remove(contextKey);
            }
        }

        public void Clear()
        {
            if (HasListeners)
            {
                KeyValuePair<IMetadataContextKey, object?>[] oldValues;
                lock (_values)
                {
                    oldValues = _values.ToArray();
                    _values.Clear();
                }

                for (int i = 0; i < oldValues.Length; i++)
                {
                    var pair = oldValues[i];
                    OnRemoved(pair.Key, pair.Value);
                }
            }
            else
            {
                lock (_values)
                {
                    _values.Clear();
                }
            }
        }

        #endregion

        #region Methods

        public void Add<T>(IMetadataContextKey<T> constant, T value)
        {
            Set(constant, value);
        }

        private void OnAdded(IMetadataContextKey key, object? newValue)
        {
            var items = GetListenersInternal();
            if (items != null)
            {
                for (var i = 0; i < items.Length; i++)
                    items[i]?.OnAdded(this, key, newValue);
            }

            _internalListener?.OnAdded(this, key, newValue);
        }

        private void OnChanged(IMetadataContextKey key, object? oldValue, object? newValue)
        {
            var items = GetListenersInternal();
            if (items != null)
            {
                for (var i = 0; i < items.Length; i++)
                    items[i]?.OnChanged(this, key, oldValue, newValue);
            }

            _internalListener?.OnChanged(this, key, oldValue, newValue);
        }

        private void OnRemoved(IMetadataContextKey key, object? oldValue)
        {
            var items = GetListenersInternal();
            if (items != null)
            {
                for (var i = 0; i < items.Length; i++)
                    items[i]?.OnRemoved(this, key, oldValue);
            }

            _internalListener?.OnRemoved(this, key, oldValue);
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

            [DataMember(Name = "L")]
            internal IList<IObservableMetadataContextListener?>? Listeners;

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

                Listeners = _metadataContext.GetListenersWithLockInternal(out int size).ToSerializable(serializationContext.Serializer, size);
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
                if (_metadataContext != null)
                    return new MementoResult(_metadataContext, serializationContext.Metadata);

                lock (this)
                {
                    if (_metadataContext != null)
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

                    if (Listeners != null)
                    {
                        foreach (var listener in Listeners)
                        {
                            if (listener != null)
                                _metadataContext.AddListener(listener);
                        }
                    }

                    return new MementoResult(_metadataContext, serializationContext.Metadata);
                }
            }

            #endregion
        }

        #endregion
    }
}