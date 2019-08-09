using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Attributes;
using MugenMvvm.Collections;
using MugenMvvm.Delegates;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Metadata.Components;

namespace MugenMvvm.Metadata.Components
{
    public class MetadataContextProviderComponent : IMetadataContextProviderComponent
    {
        #region Constructors

        [Preserve(Conditional = true)]
        public MetadataContextProviderComponent(IComponentCollectionProvider componentCollectionProvider)
        {
            Should.NotBeNull(componentCollectionProvider, nameof(componentCollectionProvider));
            ComponentCollectionProvider = componentCollectionProvider;
        }

        #endregion

        #region Properties

        protected static IComponentCollectionProvider ComponentCollectionProvider { get; private set; }

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        public virtual IReadOnlyMetadataContext? TryGetReadOnlyMetadataContext(object? target, IEnumerable<MetadataContextValue>? values)
        {
            if (values == null)
                return Default.Metadata;

            int capacity;
            if (values is IReadOnlyCollection<MetadataContextValue> readOnlyCollection)
                capacity = readOnlyCollection.Count;
            else if (values is ICollection<MetadataContextValue> collection)
                capacity = collection.Count;
            else
                capacity = 3;

            if (capacity == 0)
                return Default.Metadata;

            return new ReadOnlyMetadataContext(values, capacity);
        }

        public virtual IMetadataContext? TryGetMetadataContext(object? target, IEnumerable<MetadataContextValue>? values)
        {
            var metadataContext = new MetadataContext();
            if (values != null)
                metadataContext.Merge(values);
            return metadataContext;
        }

        int IComponent.GetPriority(object source)
        {
            return Priority;
        }

        #endregion

        #region Nested types

        protected sealed class ReadOnlyMetadataContext : LightDictionaryBase<IMetadataContextKey, object?>, IReadOnlyMetadataContext
        {
            #region Constructors

            public ReadOnlyMetadataContext(IEnumerable<MetadataContextValue> values, int capacity) : base(capacity)
            {
                foreach (var contextValue in values)
                    this[contextValue.ContextKey] = contextValue.Value;
            }

            #endregion

            #region Implementation of interfaces

            IEnumerator IEnumerable.GetEnumerator()
            {
                return ((IReadOnlyMetadataContext)this).GetEnumerator();
            }

            IEnumerator<MetadataContextValue> IEnumerable<MetadataContextValue>.GetEnumerator()
            {
                return this.Select(MetadataContextValue.CreateDelegate).GetEnumerator();
            }

            public bool TryGet<T>(IMetadataContextKey<T> contextKey, out T value, T defaultValue = default)
            {
                if (TryGetValue(contextKey, out var objValue))
                {
                    value = contextKey.GetValue(this, objValue);
                    return true;
                }

                value = contextKey.GetDefaultValue(this, defaultValue);
                return false;
            }

            public bool Contains(IMetadataContextKey contextKey)
            {
                return ContainsKey(contextKey);
            }

            #endregion

            #region Methods

            protected override bool Equals(IMetadataContextKey x, IMetadataContextKey y)
            {
                return x.Equals(y);
            }

            protected override int GetHashCode(IMetadataContextKey key)
            {
                return key.GetHashCode();
            }

            #endregion
        }

        protected sealed class MetadataContext : LightDictionaryBase<IMetadataContextKey, object?>, IMetadataContext
        {
            #region Fields

            private IComponentCollection<IComponent<IMetadataContext>>? _components;

            #endregion

            #region Constructors

            public MetadataContext()
                : base(3)
            {
            }

            #endregion

            #region Properties

            int IReadOnlyCollection<MetadataContextValue>.Count
            {
                get
                {
                    lock (this)
                    {
                        return Count;
                    }
                }
            }

            public bool HasComponents => _components != null && _components.HasItems;

            public IComponentCollection<IComponent<IMetadataContext>> Components
            {
                get
                {
                    if (_components == null)
                        ComponentCollectionProvider.LazyInitialize(ref _components, this);
                    return _components;
                }
            }

            #endregion

            #region Implementation of interfaces

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            IEnumerator<MetadataContextValue> IEnumerable<MetadataContextValue>.GetEnumerator()
            {
                IEnumerable<MetadataContextValue> list;
                lock (this)
                {
                    list = this.ToArray(MetadataContextValue.CreateDelegate);
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
                    hasValue = TryGetValue(contextKey, out obj);
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
                    return ContainsKey(contextKey);
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
                lock (this)
                {
                    if (TryGetValue(contextKey, out oldValue))
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
                    this[contextKey] = newValue;
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
                lock (this)
                {
                    if (TryGetValue(contextKey, out oldValue))
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
                    this[contextKey] = newValue;
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
                    if (TryGetValue(contextKey, out valueObj))
                    {
                        value = contextKey.GetValue(this, valueObj);
                        added = false;
                    }
                    else
                    {
                        valueObj = contextKey.SetValue(this, null, value);
                        this[contextKey] = valueObj;
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
                lock (this)
                {
                    if (TryGetValue(contextKey, out value))
                    {
                        newValue = contextKey.GetValue(this, value);
                        added = false;
                    }
                    else
                    {
                        newValue = valueFactory(this, state1, state2);
                        value = contextKey.SetValue(this, null, newValue);
                        this[contextKey] = value;
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
                    hasOldValue = TryGetValue(contextKey, out oldValue);
                    newValue = contextKey.SetValue(this, oldValue, value);
                    this[contextKey] = newValue;
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
                            if (TryGetValue(item.ContextKey, out var oldValue))
                                value = new KeyValuePair<MetadataContextValue, object?>(item, oldValue);
                            else
                                value = new KeyValuePair<MetadataContextValue, object?>(item, this);
                            values.Add(value);
                            this[item.ContextKey] = item.Value;
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
                            this[item.ContextKey] = item.Value;
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
                        changed = TryGetValue(contextKey, out oldValue) && Remove(contextKey);
                    }

                    if (changed)
                        OnRemoved(contextKey, oldValue);
                    return changed;
                }

                lock (this)
                {
                    return Count != 0 && Remove(contextKey);
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
                        oldValues = ToArray();
                        Clear();
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
                        Clear();
                    }
                }
            }

            #endregion

            #region Methods

            protected override bool Equals(IMetadataContextKey x, IMetadataContextKey y)
            {
                return x.Equals(y);
            }

            protected override int GetHashCode(IMetadataContextKey key)
            {
                return key.GetHashCode();
            }

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

        #endregion
    }
}