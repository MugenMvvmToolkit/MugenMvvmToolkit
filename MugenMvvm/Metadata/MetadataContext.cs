using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
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

        private readonly Dictionary<IMetadataContextKey, object?> _dictionary;
        private IComponentCollection? _components;

        #endregion

        #region Constructors

        public MetadataContext(ItemOrList<MetadataContextValue, IReadOnlyCollection<MetadataContextValue>> values)
        {
            var item = values.Item;
            var list = values.List;
            if (list == null)
            {
                _dictionary = new Dictionary<IMetadataContextKey, object?>(3, InternalComparer.MetadataContextKey);
                if (!item.IsEmpty)
                    _dictionary[item.ContextKey] = item.Value;
            }
            else
            {
                _dictionary = new Dictionary<IMetadataContextKey, object?>(list.Count, InternalComparer.MetadataContextKey);
                foreach (var contextValue in list)
                    _dictionary[contextValue.ContextKey] = contextValue.Value;
            }
        }

        public MetadataContext(IReadOnlyCollection<MetadataContextValue>? values = null)
            : this(ItemOrList.FromList<MetadataContextValue, IReadOnlyCollection<MetadataContextValue>>(values))
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
                    return _dictionary.Count + GetComponents().GetCount(this);
                }
            }
        }

        bool IComponentOwner.HasComponents => _components != null && _components.Count != 0;

        public IComponentCollection Components
        {
            get
            {
                if (_components == null)
                    MugenService.ComponentCollectionManager.LazyInitialize(ref _components, this);
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

                var contextValues = ItemOrListEditor.Get<MetadataContextValue>(value => value.IsEmpty);
                foreach (var keyValuePair in _dictionary)
                    contextValues.Add(MetadataContextValue.Create(keyValuePair));
                for (var i = 0; i < components.Length; i++)
                {
                    foreach (var keyValuePair in components[i].GetValues(this))
                        contextValues.Add(MetadataContextValue.Create(keyValuePair));
                }

                var v = contextValues.ToItemOrList();
                if (!v.Item.IsEmpty)
                    return Default.SingleValueEnumerator(v.Item);
                return v.List?.GetEnumerator() ?? Enumerable.Empty<MetadataContextValue>().GetEnumerator();
            }
        }

        public bool TryGetRaw(IMetadataContextKey contextKey, out object? value)
        {
            Should.NotBeNull(contextKey, nameof(contextKey));
            var components = GetComponents();
            lock (_dictionary)
            {
                return TryGet(components, contextKey, out value);
            }
        }

        public bool Contains(IMetadataContextKey contextKey)
        {
            Should.NotBeNull(contextKey, nameof(contextKey));
            var components = GetComponents();
            lock (_dictionary)
            {
                return _dictionary.ContainsKey(contextKey) || components.Contains(this, contextKey);
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
                    components.Clear(this);
                }

                return;
            }

            var oldValues = ItemOrListEditor.Get<KeyValuePair<IMetadataContextKey, object?>>(pair => pair.Key == null);
            lock (_dictionary)
            {
                foreach (var pair in _dictionary)
                    oldValues.Add(pair);
                for (var i = 0; i < components.Length; i++)
                {
                    foreach (var keyValuePair in components[i].GetValues(this))
                        oldValues.Add(keyValuePair);
                }

                _dictionary.Clear();
                components.Clear(this);
            }

            if (oldValues.Count != 0)
            {
                for (var i = 0; i < oldValues.Count; i++)
                {
                    var pair = oldValues[i];
                    listeners.OnRemoved(this, pair.Key, pair.Value);
                }
            }
        }

        #endregion

        #region Methods

        private bool TryGet(IMetadataContextValueManagerComponent[] components, IMetadataContextKey contextKey, out object? rawValue)
        {
            return components.TryGetValue(this, contextKey, out rawValue) || _dictionary.TryGetValue(contextKey, out rawValue);
        }

        private void Set(IMetadataContextValueManagerComponent[] components, IMetadataContextKey contextKey, object? rawValue)
        {
            if (!components.TrySetValue(this, contextKey, rawValue))
                _dictionary[contextKey] = rawValue;
        }

        private bool Remove(IMetadataContextValueManagerComponent[] components, IMetadataContextKey contextKey)
        {
            var remove = _dictionary.Remove(contextKey);
            return components.TryClear(this, contextKey) || remove;
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