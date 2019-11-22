using System;
using System.Collections;
using System.Collections.Generic;
using MugenMvvm.Collections.Internal;
using MugenMvvm.Delegates;
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
        private IComponentCollection<IComponent<IMetadataContext>>? _components;

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
                lock (this)
                {
                    return _dictionary.Count;
                }
            }
        }

        public bool HasComponents => _components != null && _components.Count != 0;

        public IComponentCollection<IComponent<IMetadataContext>> Components
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
            lock (this)
            {
                list = _dictionary.ToArray(MetadataContextValue.CreateDelegate);
            }

            return list.GetEnumerator();
        }

        public bool TryGet<T>(IMetadataContextKey<T> contextKey, out T value, T defaultValue = default)
        {
            Should.NotBeNull(contextKey, nameof(contextKey));
            object? obj;
            bool hasValue;
            lock (this)
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
            lock (this)
            {
                return _dictionary.ContainsKey(contextKey);
            }
        }

        public T AddOrUpdate<T, TState>(IMetadataContextKey<T> contextKey, T addValue, TState state,
            UpdateValueDelegate<IMetadataContext, T, T, TState> updateValueFactory)
        {
            Should.NotBeNull(contextKey, nameof(contextKey));
            Should.NotBeNull(updateValueFactory, nameof(updateValueFactory));
            T result;
            object? newValue, oldValue;
            bool added;
            lock (this)
            {
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
            return result;
        }

        public T AddOrUpdate<T, TState>(IMetadataContextKey<T> contextKey, TState state, Func<IMetadataContext, TState, T> valueFactory,
            UpdateValueDelegate<IMetadataContext, Func<IMetadataContext, TState, T>, T, TState> updateValueFactory)
        {
            Should.NotBeNull(contextKey, nameof(contextKey));
            Should.NotBeNull(updateValueFactory, nameof(updateValueFactory));
            T result;
            object? newValue, oldValue;
            bool added;
            lock (this)
            {
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
            return result;
        }

        public T GetOrAdd<T>(IMetadataContextKey<T> contextKey, T value)
        {
            Should.NotBeNull(contextKey, nameof(contextKey));
            object? valueObj;
            bool added;
            lock (this)
            {
                if (_dictionary.TryGetValue(contextKey, out valueObj))
                {
                    value = contextKey.GetValue(this, valueObj);
                    added = false;
                }
                else
                {
                    valueObj = contextKey.SetValue(this, null, value);
                    _dictionary[contextKey] = valueObj;
                    added = true;
                }
            }

            if (added)
                OnAdded(contextKey, valueObj);
            return value;
        }

        public T GetOrAdd<T, TState>(IMetadataContextKey<T> contextKey, TState state, Func<IMetadataContext, TState, T> valueFactory)
        {
            Should.NotBeNull(contextKey, nameof(contextKey));
            Should.NotBeNull(valueFactory, nameof(valueFactory));
            object? value;
            bool added;
            T newValue;
            lock (this)
            {
                if (_dictionary.TryGetValue(contextKey, out value))
                {
                    newValue = contextKey.GetValue(this, value);
                    added = false;
                }
                else
                {
                    newValue = valueFactory(this, state);
                    value = contextKey.SetValue(this, null, newValue);
                    _dictionary[contextKey] = value;
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
            lock (this)
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
                lock (this)
                {
                    foreach (var item in items)
                    {
                        KeyValuePair<MetadataContextValue, object?> value;
                        if (_dictionary.TryGetValue(item.ContextKey, out var oldValue))
                            value = new KeyValuePair<MetadataContextValue, object?>(item, oldValue);
                        else
                            value = new KeyValuePair<MetadataContextValue, object?>(item, this);
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
                lock (this)
                {
                    foreach (var item in items)
                        _dictionary[item.ContextKey] = item.Value;
                }
            }
        }

        bool IMetadataContext.Remove(IMetadataContextKey contextKey)
        {
            Should.NotBeNull(contextKey, nameof(contextKey));
            if (HasComponents)
            {
                bool changed;
                object? oldValue;
                lock (this)
                {
                    if (Count == 0)
                        return false;
                    changed = _dictionary.TryGetValue(contextKey, out oldValue) && _dictionary.Remove(contextKey);
                }

                if (changed)
                    OnRemoved(contextKey, oldValue);
                return changed;
            }

            lock (this)
            {
                return Count != 0 && _dictionary.Remove(contextKey);
            }
        }

        void IMetadataContext.Clear()
        {
            if (HasComponents)
            {
                KeyValuePair<IMetadataContextKey, object?>[] oldValues;
                lock (this)
                {
                    if (Count == 0)
                        return;
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
                lock (this)
                {
                    _dictionary.Clear();
                }
            }
        }

        #endregion

        #region Methods

        private void OnAdded(IMetadataContextKey key, object? newValue)
        {
            var items = this.GetComponents();
            for (var i = 0; i < items.Length; i++)
                (items[i] as IMetadataContextListener)?.OnAdded(this, key, newValue);
        }

        private void OnChanged(IMetadataContextKey key, object? oldValue, object? newValue)
        {
            var items = this.GetComponents();
            for (var i = 0; i < items.Length; i++)
                (items[i] as IMetadataContextListener)?.OnChanged(this, key, oldValue, newValue);
        }

        private void OnRemoved(IMetadataContextKey key, object? oldValue)
        {
            var items = this.GetComponents();
            for (var i = 0; i < items.Length; i++)
                (items[i] as IMetadataContextListener)?.OnRemoved(this, key, oldValue);
        }

        #endregion
    }
}