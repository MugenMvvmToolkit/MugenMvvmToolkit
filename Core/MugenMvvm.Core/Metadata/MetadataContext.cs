﻿using System;
using System.Collections;
using System.Collections.Generic;
using MugenMvvm.Collections.Internal;
using MugenMvvm.Delegates;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Metadata.Components;
using MugenMvvm.Internal;

namespace MugenMvvm.Metadata
{
    public sealed class MetadataContext : IMetadataContext//todo add new component support
    {
        #region Fields

        private readonly MetadataContextLightDictionary _dictionary;
        private IComponentCollection? _components;

        #endregion

        #region Constructors

        public MetadataContext(ItemOrList<MetadataContextValue, IReadOnlyCollection<MetadataContextValue>> values)
        {
            var item = values.Item;
            var list = values.List;
            if (list == null)
            {
                _dictionary = new MetadataContextLightDictionary(3);
                if (!item.IsEmpty)
                    _dictionary[item.ContextKey] = item.Value;
            }
            else
            {
                _dictionary = new MetadataContextLightDictionary(list.Count);
                foreach (var contextValue in list)
                    _dictionary[contextValue.ContextKey] = contextValue.Value;
            }
        }

        public MetadataContext(IReadOnlyCollection<MetadataContextValue>? values = null)
            : this(new ItemOrList<MetadataContextValue, IReadOnlyCollection<MetadataContextValue>>(values))
        {
        }

        #endregion

        #region Properties

        public int Count
        {
            get
            {
                lock (_dictionary)
                {
                    return _dictionary.Count;
                }
            }
        }

        public bool HasComponents => _components != null && _components.Count != 0;

        public IComponentCollection Components
        {
            get
            {
                if (_components == null)
                    MugenService.ComponentCollectionProvider.LazyInitialize(ref _components, this);
                return _components;
            }
        }

        #endregion

        #region Implementation of interfaces

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<MetadataContextValue> GetEnumerator()
        {
            IEnumerable<MetadataContextValue> list;
            lock (_dictionary)
            {
                list = _dictionary.ToArray(MetadataContextValue.CreateDelegate);
            }

            return list.GetEnumerator();
        }

        public bool TryGet<T>(IReadOnlyMetadataContextKey<T> contextKey, out T value, T defaultValue = default)
        {
            Should.NotBeNull(contextKey, nameof(contextKey));
            object? obj;
            bool hasValue;
            lock (_dictionary)
            {
                hasValue = _dictionary.TryGetValue(contextKey, out obj);
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
            lock (_dictionary)
            {
                return _dictionary.ContainsKey(contextKey);
            }
        }

        public TGet AddOrUpdate<TGet, TSet, TState>(IMetadataContextKey<TGet, TSet> contextKey, TSet addValue, TState state, UpdateValueDelegate<IMetadataContext, TSet, TGet, TState, TSet> updateValueFactory)
        {
            Should.NotBeNull(contextKey, nameof(contextKey));
            Should.NotBeNull(updateValueFactory, nameof(updateValueFactory));
            object? newValue, oldValue;
            bool added;
            lock (_dictionary)
            {
                TSet result;
                if (_dictionary.TryGetValue(contextKey, out oldValue))
                {
                    result = updateValueFactory(this, addValue, contextKey.GetValue(this, oldValue), state);
                    added = false;
                }
                else
                {
                    result = addValue;
                    added = true;
                }

                newValue = contextKey.SetValue(this, oldValue, result);
                _dictionary[contextKey] = newValue;
            }

            if (added)
                OnAdded(contextKey, newValue);
            else
                OnChanged(contextKey, oldValue, newValue);
            return contextKey.GetValue(this, newValue);
        }

        public TGet AddOrUpdate<TGet, TSet, TState>(IMetadataContextKey<TGet, TSet> contextKey, TState state, Func<IMetadataContext, TState, TSet> valueFactory,
            UpdateValueDelegate<IMetadataContext, TGet, TState, TSet> updateValueFactory)
        {
            Should.NotBeNull(contextKey, nameof(contextKey));
            Should.NotBeNull(updateValueFactory, nameof(updateValueFactory));
            object? newValue, oldValue;
            bool added;
            lock (_dictionary)
            {
                TSet result;
                if (_dictionary.TryGetValue(contextKey, out oldValue))
                {
                    result = updateValueFactory(this, valueFactory, contextKey.GetValue(this, oldValue), state);
                    added = false;
                }
                else
                {
                    result = valueFactory(this, state);
                    added = true;
                }

                newValue = contextKey.SetValue(this, oldValue, result);
                _dictionary[contextKey] = newValue;
            }

            if (added)
                OnAdded(contextKey, newValue);
            else
                OnChanged(contextKey, oldValue, newValue);
            return contextKey.GetValue(this, newValue);
        }

        public TGet GetOrAdd<TGet, TSet>(IMetadataContextKey<TGet, TSet> contextKey, TSet value)
        {
            Should.NotBeNull(contextKey, nameof(contextKey));
            object? valueObj;
            bool added;
            lock (_dictionary)
            {
                if (_dictionary.TryGetValue(contextKey, out valueObj))
                    added = false;
                else
                {
                    valueObj = contextKey.SetValue(this, null, value);
                    _dictionary[contextKey] = valueObj;
                    added = true;
                }
            }

            if (added)
                OnAdded(contextKey, valueObj);
            return contextKey.GetValue(this, valueObj);
        }

        public TGet GetOrAdd<TGet, TSet, TState>(IMetadataContextKey<TGet, TSet> contextKey, TState state, Func<IMetadataContext, TState, TSet> valueFactory)
        {
            Should.NotBeNull(contextKey, nameof(contextKey));
            Should.NotBeNull(valueFactory, nameof(valueFactory));
            object? value;
            bool added;
            lock (_dictionary)
            {
                if (_dictionary.TryGetValue(contextKey, out value))
                    added = false;
                else
                {
                    value = contextKey.SetValue(this, null, valueFactory(this, state));
                    _dictionary[contextKey] = value;
                    added = true;
                }
            }

            if (added)
                OnAdded(contextKey, value);
            return contextKey.GetValue(this, value);
        }

        public void Set<TGet, TSet>(IMetadataContextKey<TGet, TSet> contextKey, TSet value)
        {
            Should.NotBeNull(contextKey, nameof(contextKey));
            object? oldValue, newValue;
            bool hasOldValue;
            lock (_dictionary)
            {
                hasOldValue = _dictionary.TryGetValue(contextKey, out oldValue);
                newValue = contextKey.SetValue(this, oldValue, value);
                _dictionary[contextKey] = newValue;
            }

            if (hasOldValue)
                OnChanged(contextKey, oldValue, newValue);
            else
                OnAdded(contextKey, newValue);
        }

        public void Merge(IEnumerable<MetadataContextValue> items)
        {
            Should.NotBeNull(items, nameof(items));
            if (HasComponents)
            {
                var values = new List<KeyValuePair<MetadataContextValue, object?>>();
                lock (_dictionary)
                {
                    foreach (var item in items)
                    {
                        var value = _dictionary.TryGetValue(item.ContextKey, out var oldValue)
                            ? new KeyValuePair<MetadataContextValue, object?>(item, oldValue)
                            : new KeyValuePair<MetadataContextValue, object?>(item, this);
                        values.Add(value);
                        _dictionary[item.ContextKey] = item.Value;
                    }
                }

                for (var index = 0; index < values.Count; index++)
                {
                    var pair = values[index];
                    if (ReferenceEquals(pair.Value, this))
                        OnAdded(pair.Key.ContextKey, pair.Key.Value);
                    else
                        OnChanged(pair.Key.ContextKey, pair.Value, pair.Key.Value);
                }
            }
            else
            {
                lock (_dictionary)
                {
                    foreach (var item in items)
                        _dictionary[item.ContextKey] = item.Value;
                }
            }
        }

        public bool Clear(IMetadataContextKey? contextKey = null)
        {
            if (contextKey != null)
                return ClearInternal(contextKey);
            if (HasComponents)
            {
                KeyValuePair<IMetadataContextKey, object?>[] oldValues;
                lock (_dictionary)
                {
                    if (_dictionary.Count == 0)
                        return false;
                    oldValues = _dictionary.ToArray();
                    _dictionary.Clear();
                }

                for (var i = 0; i < oldValues.Length; i++)
                {
                    var pair = oldValues[i];
                    OnRemoved(pair.Key, pair.Value);
                }
            }
            else
            {
                lock (_dictionary)
                {
                    if (_dictionary.Count == 0)
                        return false;
                    _dictionary.Clear();
                }
            }

            return true;
        }

        #endregion

        #region Methods

        private bool ClearInternal(IMetadataContextKey contextKey)
        {
            if (HasComponents)
            {
                bool changed;
                object? oldValue;
                lock (_dictionary)
                {
                    if (Count == 0)
                        return false;
                    changed = _dictionary.TryGetValue(contextKey, out oldValue) && _dictionary.Remove(contextKey);
                }

                if (changed)
                    OnRemoved(contextKey, oldValue);
                return changed;
            }

            lock (_dictionary)
            {
                return Count != 0 && _dictionary.Remove(contextKey);
            }
        }

        private void OnAdded(IMetadataContextKey key, object? newValue)
        {
            var items = _components.GetOrDefault<IMetadataContextListener>(null);
            for (var i = 0; i < items.Length; i++)
                items[i].OnAdded(this, key, newValue);
        }

        private void OnChanged(IMetadataContextKey key, object? oldValue, object? newValue)
        {
            var items = _components.GetOrDefault<IMetadataContextListener>(null);
            for (var i = 0; i < items.Length; i++)
                items[i].OnChanged(this, key, oldValue, newValue);
        }

        private void OnRemoved(IMetadataContextKey key, object? oldValue)
        {
            var items = _components.GetOrDefault<IMetadataContextListener>(null);
            for (var i = 0; i < items.Length; i++)
                items[i].OnRemoved(this, key, oldValue);
        }

        #endregion
    }
}