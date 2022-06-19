﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Collections;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Metadata.Components;
using MugenMvvm.Internal;

namespace MugenMvvm.Metadata
{
    public sealed class MetadataContext : IMetadataContext, IReadOnlyCollection<KeyValuePair<IMetadataContextKey, object?>>//todo check equality
    {
        private readonly Dictionary<IMetadataContextKey, object?> _dictionary;
        private IComponentCollection? _components;

        public MetadataContext(ItemOrIEnumerable<KeyValuePair<IMetadataContextKey, object?>> values)
        {
            _dictionary = new Dictionary<IMetadataContextKey, object?>(3, InternalEqualityComparer.MetadataContextKey);
            foreach (var value in values)
                _dictionary[value.Key] = value.Value;
        }

        public MetadataContext(IReadOnlyMetadataContext? metadata)
        {
            if (metadata == null)
            {
                _dictionary = new Dictionary<IMetadataContextKey, object?>(3, InternalEqualityComparer.MetadataContextKey);
                return;
            }

            _dictionary = new Dictionary<IMetadataContextKey, object?>(metadata.Count, InternalEqualityComparer.MetadataContextKey);
            foreach (var pair in metadata.GetValues())
                _dictionary[pair.Key] = pair.Value;
        }

        public MetadataContext(IEnumerable<KeyValuePair<IMetadataContextKey, object?>>? values = null)
            : this(ItemOrIEnumerable.FromList(values))
        {
        }

        public IComponentCollection Components => _components ?? MugenService.ComponentCollectionManager.EnsureInitialized(ref _components, this);

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

        public ItemOrListEnumerator<KeyValuePair<IMetadataContextKey, object?>> GetEnumerator() => GetValues().GetEnumerator();

        public void Add<T>(IMetadataContextKey<T> contextKey, T value) => Set(contextKey, value, out _);

        public T AddOrUpdate<T, TState>(IMetadataContextKey<T> contextKey, T addValue, TState state,
            Func<IMetadataContext, IMetadataContextKey<T>, object?, TState, T> updateValueFactory)
        {
            Should.NotBeNull(contextKey, nameof(contextKey));
            Should.NotBeNull(updateValueFactory, nameof(updateValueFactory));
            object? newValue, oldValue;
            bool added;
            var components = GetComponents();
            lock (_dictionary)
            {
                if (TryGet(components, contextKey, MetadataOperationType.Get, out oldValue))
                {
                    addValue = updateValueFactory(this, contextKey, oldValue, state);
                    added = false;
                }
                else
                    added = true;

                newValue = contextKey.SetValue(this, oldValue, addValue);
                Set(components, contextKey, newValue);
            }

            if (added)
                GetListeners().OnAdded(this, contextKey, newValue);
            else
                GetListeners().OnChanged(this, contextKey, oldValue, newValue);
            return contextKey.GetValue(this, newValue, addValue);
        }

        public T AddOrUpdate<T, TState>(IMetadataContextKey<T> contextKey, TState state, Func<IMetadataContext, IMetadataContextKey<T>, TState, T> valueFactory,
            Func<IMetadataContext, IMetadataContextKey<T>, object?, TState, T> updateValueFactory)
        {
            Should.NotBeNull(contextKey, nameof(contextKey));
            Should.NotBeNull(updateValueFactory, nameof(updateValueFactory));
            object? newValueRaw, oldValue;
            bool added;
            T newValue;
            var components = GetComponents();
            lock (_dictionary)
            {
                if (TryGet(components, contextKey, MetadataOperationType.Get, out oldValue))
                {
                    newValue = updateValueFactory(this, contextKey, oldValue, state);
                    added = false;
                }
                else
                {
                    newValue = valueFactory(this, contextKey, state);
                    added = true;
                }

                newValueRaw = contextKey.SetValue(this, oldValue, newValue);
                Set(components, contextKey, newValueRaw);
            }

            if (added)
                GetListeners().OnAdded(this, contextKey, newValueRaw);
            else
                GetListeners().OnChanged(this, contextKey, oldValue, newValueRaw);
            return contextKey.GetValue(this, newValueRaw, newValue);
        }

        public T GetOrAdd<T>(IMetadataContextKey<T> contextKey, T value)
        {
            Should.NotBeNull(contextKey, nameof(contextKey));
            bool added;
            var components = GetComponents();
            object? newValueRaw;
            lock (_dictionary)
            {
                if (TryGet(components, contextKey, MetadataOperationType.Get, out var oldValue))
                {
                    added = false;
                    newValueRaw = null;
                    value = contextKey.GetValue(this, oldValue);
                }
                else
                {
                    newValueRaw = contextKey.SetValue(this, null, value);
                    Set(components, contextKey, newValueRaw);
                    added = true;
                }
            }

            if (added)
            {
                GetListeners().OnAdded(this, contextKey, newValueRaw);
                return contextKey.GetValue(this, newValueRaw, value);
            }

            return value;
        }

        public T GetOrAdd<T, TState>(IMetadataContextKey<T> contextKey, TState state, Func<IMetadataContext, IMetadataContextKey<T>, TState, T> valueFactory)
        {
            Should.NotBeNull(contextKey, nameof(contextKey));
            Should.NotBeNull(valueFactory, nameof(valueFactory));
            T value;
            object? valueRaw;
            bool added;
            var components = GetComponents();
            lock (_dictionary)
            {
                if (TryGet(components, contextKey, MetadataOperationType.Get, out var oldValue))
                {
                    added = false;
                    value = contextKey.GetValue(this, oldValue);
                    valueRaw = null;
                }
                else
                {
                    value = valueFactory(this, contextKey, state);
                    valueRaw = contextKey.SetValue(this, null, value);
                    Set(components, contextKey, valueRaw);
                    added = true;
                }
            }

            if (added)
            {
                GetListeners().OnAdded(this, contextKey, valueRaw);
                return contextKey.GetValue(this, valueRaw, value);
            }

            return value;
        }

        public bool Set<T>(IMetadataContextKey<T> contextKey, T value, out object? oldValue)
        {
            Should.NotBeNull(contextKey, nameof(contextKey));
            bool hasOldValue;
            object? valueRaw;
            var components = GetComponents();
            lock (_dictionary)
            {
                hasOldValue = TryGet(components, contextKey, MetadataOperationType.Set, out oldValue);
                valueRaw = contextKey.SetValue(this, oldValue, value);
                Set(components, contextKey, valueRaw);
            }

            if (hasOldValue)
                GetListeners().OnChanged(this, contextKey, oldValue, valueRaw);
            else
                GetListeners().OnAdded(this, contextKey, valueRaw);
            return hasOldValue;
        }

        public void Merge(ItemOrIEnumerable<KeyValuePair<IMetadataContextKey, object?>> values)
        {
            var components = GetComponents();
            var listeners = GetListeners();
            if (listeners.Count == 0)
            {
                lock (_dictionary)
                {
                    foreach (var item in values)
                        Set(components, item.Key, item.Value);
                }
            }
            else
            {
                var oldValues = new ItemOrListEditor<KeyValuePair<KeyValuePair<IMetadataContextKey, object?>, object?>>();
                lock (_dictionary)
                {
                    foreach (var item in values)
                    {
                        var value = TryGet(components, item.Key, MetadataOperationType.Set, out var oldValue)
                            ? new KeyValuePair<KeyValuePair<IMetadataContextKey, object?>, object?>(item, oldValue)
                            : new KeyValuePair<KeyValuePair<IMetadataContextKey, object?>, object?>(item, this);
                        oldValues.Add(value);
                        Set(components, item.Key, item.Value);
                    }
                }

                for (var index = 0; index < oldValues.Count; index++)
                {
                    var pair = oldValues[index];
                    if (pair.Value == this)
                        listeners.OnAdded(this, pair.Key.Key, pair.Key.Value);
                    else
                        listeners.OnChanged(this, pair.Key.Key, pair.Value, pair.Key.Value);
                }
            }
        }

        public bool Remove(IMetadataContextKey contextKey, out object? oldValue)
        {
            Should.NotBeNull(contextKey, nameof(contextKey));
            var components = GetComponents();
            bool removed;
            lock (_dictionary)
            {
                removed = TryGet(components, contextKey, MetadataOperationType.Remove, out oldValue) && Remove(components, contextKey);
            }

            if (removed)
                GetListeners().OnRemoved(this, contextKey, oldValue);
            return removed;
        }

        public void Clear()
        {
            var components = GetComponents();
            var listeners = GetListeners();
            if (listeners.Count == 0)
            {
                lock (_dictionary)
                {
                    _dictionary.Clear();
                    components.Clear(this);
                }

                return;
            }

            var oldValues = new ItemOrListEditor<KeyValuePair<IMetadataContextKey, object?>>();
            lock (_dictionary)
            {
                foreach (var pair in _dictionary)
                    oldValues.Add(pair);
                components.GetValues(this, MetadataOperationType.Remove, ref oldValues);

                _dictionary.Clear();
                components.Clear(this);
            }

            for (var i = 0; i < oldValues.Count; i++)
            {
                var pair = oldValues[i];
                listeners.OnRemoved(this, pair.Key, pair.Value);
            }
        }

        public ItemOrIReadOnlyCollection<KeyValuePair<IMetadataContextKey, object?>> GetValues()
        {
            var components = GetComponents();
            lock (_dictionary)
            {
                var contextValues = new ItemOrListEditor<KeyValuePair<IMetadataContextKey, object?>>();
                foreach (var keyValuePair in _dictionary)
                    contextValues.Add(keyValuePair);
                components.GetValues(this, MetadataOperationType.Get, ref contextValues);
                return contextValues.ToItemOrList();
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

        public bool TryGetRaw(IMetadataContextKey contextKey, [MaybeNullWhen(false)] out object? value)
        {
            Should.NotBeNull(contextKey, nameof(contextKey));
            var components = GetComponents();
            lock (_dictionary)
            {
                return TryGet(components, contextKey, MetadataOperationType.Get, out value);
            }
        }

        private bool TryGet(ItemOrArray<IMetadataValueManagerComponent> components, IMetadataContextKey contextKey, MetadataOperationType operationType,
            out object? rawValue) =>
            components.TryGetValue(this, contextKey, operationType, out rawValue) || _dictionary.TryGetValue(contextKey, out rawValue);

        private void Set(ItemOrArray<IMetadataValueManagerComponent> components, IMetadataContextKey contextKey, object? rawValue)
        {
            if (!components.TrySetValue(this, contextKey, rawValue))
                _dictionary[contextKey] = rawValue;
        }

        private bool Remove(ItemOrArray<IMetadataValueManagerComponent> components, IMetadataContextKey contextKey)
        {
            var remove = _dictionary.Remove(contextKey);
            return components.TryClear(this, contextKey) || remove;
        }

        private ItemOrArray<IMetadataValueManagerComponent> GetComponents() => _components?.Get<IMetadataValueManagerComponent>() ?? default;

        private ItemOrArray<IMetadataContextListener> GetListeners() => _components?.Get<IMetadataContextListener>() ?? default;

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        IEnumerator<KeyValuePair<IMetadataContextKey, object?>> IEnumerable<KeyValuePair<IMetadataContextKey, object?>>.GetEnumerator() => GetEnumerator();
    }
}