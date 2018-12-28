using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using MugenMvvm.Attributes;
using MugenMvvm.Collections;
using MugenMvvm.Infrastructure.Serialization;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Serialization;

namespace MugenMvvm.Infrastructure.Metadata
{
    public class MetadataContext : IObservableMetadataContext
    {
        #region Fields

        private readonly Dictionary<IMetadataContextKey, object?> _values;
        private LightArrayList<IObservableMetadataContextListener>? _listeners;

        #endregion

        #region Constructors

        public MetadataContext()
        {
            _values = new Dictionary<IMetadataContextKey, object?>();
        }

        private MetadataContext(Dictionary<IMetadataContextKey, object?> values)
        {
            _values = values;
        }

        public MetadataContext(IReadOnlyMetadataContext context)
            : this()
        {
            Should.NotBeNull(context, nameof(context));
            Merge(context);
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

        public IEnumerator<MetadataContextValue> GetEnumerator()
        {
            IEnumerable<MetadataContextValue> list;
            lock (_values)
            {
                list = _values.ToArray(pair => new MetadataContextValue(pair.Key, pair.Value));
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

        public bool TryGet(IMetadataContextKey contextKey, out object? value, object? defaultValue = null)
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

            value = contextKey.GetDefaultValue(this, defaultValue);
            return false;
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
                value = (T)obj;
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

        public void Set(IMetadataContextKey contextKey, object? value)
        {
            Should.NotBeNull(contextKey, nameof(contextKey));
            contextKey.Validate(value);
            lock (_values)
            {
                _values[contextKey] = value;
            }

            OnContextChanged(contextKey);
        }

        public void Set<T>(IMetadataContextKey<T> contextKey, T value)
        {
            object? obj = value;
            Set(contextKey, obj);
        }

        public void Merge(IEnumerable<MetadataContextValue> items)
        {
            Should.NotBeNull(items, nameof(items));
            var changed = false;
            lock (_values)
            {
                foreach (var item in items)
                {
                    _values[item.ContextKey] = item.Value;
                    changed = true;
                }
            }

            if (changed)
                OnContextChanged(null);
        }

        public bool Remove(IMetadataContextKey contextKey)
        {
            Should.NotBeNull(contextKey, nameof(contextKey));
            bool changed;
            lock (_values)
            {
                changed = _values.Remove(contextKey);
            }

            if (changed)
                OnContextChanged(contextKey);
            return changed;
        }

        public void Clear()
        {
            bool changed;
            lock (_values)
            {
                changed = _values.Count > 0;
                _values.Clear();
            }

            if (changed)
                OnContextChanged(null);
        }

        public void AddListener(IObservableMetadataContextListener listener)
        {
            Should.NotBeNull(listener, nameof(listener));
            if (_listeners == null)
                MugenExtensions.LazyInitialize(ref _listeners, new LightArrayList<IObservableMetadataContextListener>());
            _listeners!.AddWithLock(listener);
        }

        public void RemoveListener(IObservableMetadataContextListener listener)
        {
            Should.NotBeNull(listener, nameof(listener));
            _listeners?.RemoveWithLock(listener);
        }

        public IReadOnlyList<IObservableMetadataContextListener> GetListeners()
        {
            if (_listeners == null)
                return Default.EmptyArray<IObservableMetadataContextListener>();
            var items = _listeners.GetItemsWithLock(out var size);
            var listeners = new IObservableMetadataContextListener[size];
            for (var i = 0; i < size; i++)
                listeners[i] = items[i];
            return listeners;
        }

        #endregion

        #region Methods

        public void Add<T>(IMetadataContextKey<T> constant, T value)
        {
            Set(constant, value);
        }

        private void OnContextChanged(IMetadataContextKey? key)
        {
            var items = _listeners?.GetItems(out _);
            if (items != null)
            {
                for (var i = 0; i < items.Length; i++)
                    items[i]?.OnContextChanged(this, key);
            }
        }

        #endregion

        #region Nested types

        [Serializable]
        [DataContract(Namespace = BuildConstants.DataContractNamespace)]
        [Preserve(Conditional = true, AllMembers = true)]
        public sealed class ContextMemento : IMemento
        {
            #region Fields

            [IgnoreDataMember] [XmlIgnore] private MetadataContext? _metadataContext;

            [DataMember(Name = "K")] internal IList<IMetadataContextKey>? Keys;

            [DataMember(Name = "L")] internal IList<IObservableMetadataContextListener?>? Listeners;

            [DataMember(Name = "V")] internal IList<object?>? Values;

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

                Listeners = _metadataContext._listeners?.GetItems(out int size).ToSerializable(serializationContext.Serializer, size);
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
                    for (var i = 0; i < Keys!.
                    Count;
                    i++)
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