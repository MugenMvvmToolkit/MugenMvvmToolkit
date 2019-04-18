using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Delegates;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Infrastructure.Metadata
{
    public sealed class MetadataContext : IObservableMetadataContext
    {
        #region Fields

        private readonly Dictionary<IMetadataContextKey, object?> _values;
        private IComponentCollection<IObservableMetadataContextListener>? _listeners;

        #endregion

        #region Constructors

        private MetadataContext(Dictionary<IMetadataContextKey, object?> values)
        {
            _values = values;
        }

        public MetadataContext()
            : this(new Dictionary<IMetadataContextKey, object?>())
        {
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

        public IComponentCollection<IObservableMetadataContextListener> Listeners
        {
            get
            {
                if (_listeners == null)
                    MugenExtensions.LazyInitialize(ref _listeners, this);
                return _listeners;
            }
        }

        private bool HasListeners => _listeners != null && Listeners.HasItems;

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

        public T AddOrUpdate<T, TState1, TState2>(IMetadataContextKey<T> contextKey, T addValue, TState1 state1, TState2 state2,
            UpdateValueDelegate<IMetadataContext, T, T, TState1, TState2> updateValueFactory)
        {
            Should.NotBeNull(contextKey, nameof(contextKey));
            Should.NotBeNull(updateValueFactory, nameof(updateValueFactory));
            T result;
            object? newValue, oldValue;
            bool added;
            lock (_values)
            {
                if (_values.TryGetValue(contextKey, out oldValue))
                {
                    result = updateValueFactory(this, addValue, contextKey.GetValue(this, oldValue), state1, state2);
                    added = false;
                }
                else
                {
                    result = addValue;
                    added = true;
                }

                newValue = contextKey.SetValue(this, oldValue, result);
                _values[contextKey] = newValue;
            }

            if (added)
                OnAdded(contextKey, newValue);
            else
                OnChanged(contextKey, oldValue, newValue);
            return result;
        }

        public T AddOrUpdate<T, TState1, TState2>(IMetadataContextKey<T> contextKey, TState1 state1, TState2 state2, Func<IMetadataContext, TState1, TState2, T> valueFactory,
            UpdateValueDelegate<IMetadataContext, Func<IMetadataContext, TState1, TState2, T>, T, TState1, TState2> updateValueFactory)
        {
            Should.NotBeNull(contextKey, nameof(contextKey));
            Should.NotBeNull(updateValueFactory, nameof(updateValueFactory));
            T result;
            object? newValue, oldValue;
            bool added;
            lock (_values)
            {
                if (_values.TryGetValue(contextKey, out oldValue))
                {
                    result = updateValueFactory(this, valueFactory, contextKey.GetValue(this, oldValue), state1, state2);
                    added = false;
                }
                else
                {
                    result = valueFactory(this, state1, state2);
                    added = true;
                }

                newValue = contextKey.SetValue(this, oldValue, result);
                _values[contextKey] = newValue;
            }

            if (added)
                OnAdded(contextKey, newValue);
            else
                OnChanged(contextKey, oldValue, newValue);
            return result;
        }

        public T GetOrAdd<T>(IMetadataContextKey<T> contextKey, T value)
        {
            Should.NotBeNull(contextKey, nameof(contextKey));
            object? valueObj;
            bool added;
            lock (_values)
            {
                if (_values.TryGetValue(contextKey, out valueObj))
                {
                    value = contextKey.GetValue(this, valueObj);
                    added = false;
                }
                else
                {
                    valueObj = contextKey.SetValue(this, null, value);
                    _values[contextKey] = valueObj;
                    added = true;
                }
            }

            if (added)
                OnAdded(contextKey, valueObj);
            return value;
        }

        public T GetOrAdd<T, TState1, TState2>(IMetadataContextKey<T> contextKey, TState1 state1, TState2 state2, Func<IMetadataContext, TState1, TState2, T> valueFactory)
        {
            Should.NotBeNull(contextKey, nameof(contextKey));
            Should.NotBeNull(valueFactory, nameof(valueFactory));
            object? value;
            bool added;
            T newValue;
            lock (_values)
            {
                if (_values.TryGetValue(contextKey, out value))
                {
                    newValue = contextKey.GetValue(this, value);
                    added = false;
                }
                else
                {
                    newValue = valueFactory(this, state1, state2);
                    value = contextKey.SetValue(this, null, newValue);
                    _values[contextKey] = value;
                    added = true;
                }
            }

            if (added)
                OnAdded(contextKey, value);
            return newValue;
        }

        public void Set<T>(IMetadataContextKey<T> contextKey, T value)
        {
            Should.NotBeNull(contextKey, nameof(contextKey));
            object? oldValue, newValue;
            bool hasOldValue;
            lock (_values)
            {
                hasOldValue = _values.TryGetValue(contextKey, out oldValue);
                newValue = contextKey.SetValue(this, oldValue, value);
                _values[contextKey] = newValue;
            }

            if (hasOldValue)
                OnChanged(contextKey, oldValue, newValue);
            else
                OnAdded(contextKey, newValue);
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
                    foreach (var item in items) _values[item.ContextKey] = item.Value;
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

                for (var i = 0; i < oldValues.Length; i++)
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
            if (!HasListeners)
                return;
            var items = GetListeners();
            for (var i = 0; i < items.Count; i++)
                items[i].OnAdded(this, key, newValue);
        }

        private void OnChanged(IMetadataContextKey key, object? oldValue, object? newValue)
        {
            if (!HasListeners)
                return;
            var items = GetListeners();
            for (var i = 0; i < items.Count; i++)
                items[i].OnChanged(this, key, oldValue, newValue);
        }

        private void OnRemoved(IMetadataContextKey key, object? oldValue)
        {
            if (!HasListeners)
                return;
            var items = GetListeners();
            for (var i = 0; i < items.Count; i++)
                items[i].OnRemoved(this, key, oldValue);
        }

        private IReadOnlyList<IObservableMetadataContextListener> GetListeners()
        {
            return _listeners?.GetItems() ?? Default.EmptyArray<IObservableMetadataContextListener>();
        }

        #endregion
    }
}