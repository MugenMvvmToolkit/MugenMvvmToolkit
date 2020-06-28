﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Collections.Internal;
using MugenMvvm.Delegates;
using MugenMvvm.Extensions;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Metadata.Components;
using MugenMvvm.Internal;

namespace MugenMvvm.Metadata
{
    public sealed class MetadataContext : IMetadataContext
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
                    return _dictionary.Count + GetComponents().GetCount();
                }
            }
        }

        bool IComponentOwner.HasComponents => _components != null && _components.Count != 0;

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
            var components = GetComponents();
            lock (_dictionary)
            {
                if (components.Length == 0)
                    return ((IEnumerable<MetadataContextValue>)_dictionary.ToArray(MetadataContextValue.CreateDelegate)).GetEnumerator();

                ItemOrList<MetadataContextValue, List<MetadataContextValue>> contextValues = default;
                foreach (var keyValuePair in _dictionary)
                    contextValues.Add(MetadataContextValue.Create(keyValuePair), v => v.IsEmpty);
                for (var i = 0; i < components.Length; i++)
                {
                    foreach (var keyValuePair in components[i].GetValues())
                        contextValues.Add(MetadataContextValue.Create(keyValuePair), v => v.IsEmpty);
                }

                if (!contextValues.Item.IsEmpty)
                    return Default.SingleValueEnumerator(contextValues.Item);
                return contextValues.List?.GetEnumerator() ?? Enumerable.Empty<MetadataContextValue>().GetEnumerator();
            }
        }

        public bool TryGet<T>(IReadOnlyMetadataContextKey<T> contextKey, out T value, T defaultValue = default)
        {
            Should.NotBeNull(contextKey, nameof(contextKey));
            object? obj;
            bool hasValue;
            var components = GetComponents();
            lock (_dictionary)
            {
                hasValue = TryGet(components, contextKey, out obj);
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
            var components = GetComponents();
            lock (_dictionary)
            {
                return _dictionary.ContainsKey(contextKey) || components.Contains(contextKey);
            }
        }

        public TGet AddOrUpdate<TGet, TSet, TState>(IMetadataContextKey<TGet, TSet> contextKey, TSet addValue, in TState state, UpdateValueDelegate<IMetadataContext, TSet, TGet, TState, TSet> updateValueFactory)
        {
            Should.NotBeNull(contextKey, nameof(contextKey));
            Should.NotBeNull(updateValueFactory, nameof(updateValueFactory));
            object? newValue, oldValue;
            bool added;
            var components = GetComponents();
            var listeners = GetListeners();
            lock (_dictionary)
            {
                TSet result;
                if (TryGet(components, contextKey, out oldValue))
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
                Set(components, contextKey, newValue);
            }

            if (added)
                listeners.OnAdded(this, contextKey, newValue);
            else
                listeners.OnChanged(this, contextKey, oldValue, newValue);
            return contextKey.GetValue(this, newValue);
        }

        public TGet AddOrUpdate<TGet, TSet, TState>(IMetadataContextKey<TGet, TSet> contextKey, in TState state, Func<IMetadataContext, TState, TSet> valueFactory,
            UpdateValueDelegate<IMetadataContext, TGet, TState, TSet> updateValueFactory)
        {
            Should.NotBeNull(contextKey, nameof(contextKey));
            Should.NotBeNull(updateValueFactory, nameof(updateValueFactory));
            object? newValue, oldValue;
            bool added;
            var components = GetComponents();
            var listeners = GetListeners();
            lock (_dictionary)
            {
                TSet result;
                if (TryGet(components, contextKey, out oldValue))
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
                Set(components, contextKey, newValue);
            }

            if (added)
                listeners.OnAdded(this, contextKey, newValue);
            else
                listeners.OnChanged(this, contextKey, oldValue, newValue);
            return contextKey.GetValue(this, newValue);
        }

        public TGet GetOrAdd<TGet, TSet>(IMetadataContextKey<TGet, TSet> contextKey, TSet value)
        {
            Should.NotBeNull(contextKey, nameof(contextKey));
            object? rawValue;
            bool added;
            var components = GetComponents();
            var listeners = GetListeners();
            lock (_dictionary)
            {
                if (TryGet(components, contextKey, out rawValue))
                    added = false;
                else
                {
                    rawValue = contextKey.SetValue(this, null, value);
                    Set(components, contextKey, rawValue);
                    added = true;
                }
            }

            if (added)
                listeners.OnAdded(this, contextKey, rawValue);
            return contextKey.GetValue(this, rawValue);
        }

        public TGet GetOrAdd<TGet, TSet, TState>(IMetadataContextKey<TGet, TSet> contextKey, in TState state, Func<IMetadataContext, TState, TSet> valueFactory)
        {
            Should.NotBeNull(contextKey, nameof(contextKey));
            Should.NotBeNull(valueFactory, nameof(valueFactory));
            object? value;
            bool added;
            var components = GetComponents();
            var listeners = GetListeners();
            lock (_dictionary)
            {
                if (TryGet(components, contextKey, out value))
                    added = false;
                else
                {
                    value = contextKey.SetValue(this, null, valueFactory(this, state));
                    Set(components, contextKey, value);
                    added = true;
                }
            }

            if (added)
                listeners.OnAdded(this, contextKey, value);
            return contextKey.GetValue(this, value);
        }

        public void Set<TGet, TSet>(IMetadataContextKey<TGet, TSet> contextKey, TSet value, out object? oldValue)
        {
            Should.NotBeNull(contextKey, nameof(contextKey));
            object? newValue;
            bool hasOldValue;
            var components = GetComponents();
            var listeners = GetListeners();
            lock (_dictionary)
            {
                hasOldValue = TryGet(components, contextKey, out oldValue);
                newValue = contextKey.SetValue(this, oldValue, value);
                Set(components, contextKey, newValue);
            }

            if (hasOldValue)
                listeners.OnChanged(this, contextKey, oldValue, newValue);
            else
                listeners.OnAdded(this, contextKey, newValue);
        }

        public void Merge(IEnumerable<MetadataContextValue> items)
        {
            Should.NotBeNull(items, nameof(items));
            var components = GetComponents();
            var listeners = GetListeners();
            if (listeners.Length == 0)
            {
                lock (_dictionary)
                {
                    foreach (var item in items)
                        Set(components, item.ContextKey, item.Value);
                }

                return;
            }

            var values = new List<KeyValuePair<MetadataContextValue, object?>>();
            lock (_dictionary)
            {
                foreach (var item in items)
                {
                    var value = TryGet(components, item.ContextKey, out var oldValue)
                        ? new KeyValuePair<MetadataContextValue, object?>(item, oldValue)
                        : new KeyValuePair<MetadataContextValue, object?>(item, this);
                    values.Add(value);
                    Set(components, item.ContextKey, item.Value);
                }
            }

            for (var index = 0; index < values.Count; index++)
            {
                var pair = values[index];
                if (ReferenceEquals(pair.Value, this))
                    listeners.OnAdded(this, pair.Key.ContextKey, pair.Key.Value);
                else
                    listeners.OnChanged(this, pair.Key.ContextKey, pair.Value, pair.Key.Value);
            }
        }

        public bool Clear(IMetadataContextKey contextKey, out object? oldValue)
        {
            Should.NotBeNull(contextKey, nameof(contextKey));
            var components = GetComponents();
            var listeners = GetListeners();
            bool changed;
            lock (_dictionary)
            {
                changed = TryGet(components, contextKey, out oldValue) && Remove(components, contextKey);
            }

            if (changed)
                listeners.OnRemoved(this, contextKey, oldValue);
            return changed;
        }

        public void Clear()
        {
            var components = GetComponents();
            var listeners = GetListeners();
            if (listeners.Length == 0)
            {
                lock (_dictionary)
                {
                    _dictionary.Clear();
                    components.Clear();
                }

                return;
            }

            ItemOrList<KeyValuePair<IMetadataContextKey, object?>, List<KeyValuePair<IMetadataContextKey, object?>>> oldValues = default;
            lock (_dictionary)
            {
                foreach (var pair in _dictionary)
                    oldValues.Add(pair, p => p.Key == null);
                for (var i = 0; i < components.Length; i++)
                {
                    foreach (var keyValuePair in components[i].GetValues())
                        oldValues.Add(keyValuePair, p => p.Key == null);
                }

                _dictionary.Clear();
                components.Clear();
            }

            var count = oldValues.Count(p => p.Key == null);
            if (count != 0)
            {
                for (var i = 0; i < count; i++)
                {
                    var pair = oldValues.Get(i);
                    listeners.OnRemoved(this, pair.Key, pair.Value);
                }
            }
        }

        #endregion

        #region Methods

        private bool TryGet(IMetadataContextValueManagerComponent[] components, IMetadataContextKey contextKey, out object? rawValue)
        {
            return components.TryGetValue(contextKey, out rawValue) || _dictionary.TryGetValue(contextKey, out rawValue);
        }

        private void Set(IMetadataContextValueManagerComponent[] components, IMetadataContextKey contextKey, object? rawValue)
        {
            if (!components.TrySetValue(contextKey, rawValue))
                _dictionary[contextKey] = rawValue;
        }

        private bool Remove(IMetadataContextValueManagerComponent[] components, IMetadataContextKey contextKey)
        {
            var remove = _dictionary.Remove(contextKey);
            return components.TryClear(contextKey) || remove;
        }

        private IMetadataContextValueManagerComponent[] GetComponents()
        {
            return _components.GetOrDefault<IMetadataContextValueManagerComponent>();
        }

        private IMetadataContextListener[] GetListeners()
        {
            return _components.GetOrDefault<IMetadataContextListener>();
        }

        #endregion
    }
}