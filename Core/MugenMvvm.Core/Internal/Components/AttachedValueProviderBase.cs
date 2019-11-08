using System;
using System.Collections.Generic;
using MugenMvvm.Collections;
using MugenMvvm.Delegates;
using MugenMvvm.Interfaces.Internal;

namespace MugenMvvm.Internal.Components
{
    public abstract class AttachedValueProviderBase : IAttachedValueProvider
    {
        #region Implementation of interfaces

        public virtual IReadOnlyList<KeyValuePair<string, object?>> GetValues<TItem, TState>(TItem item, TState state, Func<TItem, string, object?, TState, bool>? predicate = null)
            where TItem : class
        {
            Should.NotBeNull(item, nameof(item));
            var dictionary = GetAttachedDictionary(item);
            if (dictionary == null)
                return Default.EmptyArray<KeyValuePair<string, object?>>();
            lock (dictionary)
            {
                if (predicate == null)
                    return new List<KeyValuePair<string, object?>>(dictionary);
                var list = new List<KeyValuePair<string, object?>>();
                foreach (var keyValue in dictionary)
                {
                    if (predicate(item, keyValue.Key, keyValue.Value, state))
                        list.Add(keyValue);
                }

                return list;
            }
        }

        public virtual bool TryGetValue<TItem, TValue>(TItem item, string path, out TValue value) where TItem : class
        {
            Should.NotBeNull(item, nameof(item));
            Should.NotBeNull(path, nameof(path));
            var dictionary = GetAttachedDictionary(item);
            if (dictionary == null)
            {
                value = default!;
                return false;
            }

            lock (dictionary)
            {
                if (dictionary.TryGetValue(path, out var result))
                {
                    value = (TValue)result!;
                    return true;
                }

                value = default!;
                return false;
            }
        }

        public virtual bool Contains<TItem>(TItem item, string path) where TItem : class
        {
            Should.NotBeNull(item, nameof(item));
            var dictionary = GetAttachedDictionary(item);
            if (dictionary == null)
                return false;
            lock (dictionary)
            {
                return dictionary.ContainsKey(path);
            }
        }

        public virtual TValue AddOrUpdate<TItem, TValue, TState>(TItem item, string path, TValue addValue, TState state, UpdateValueDelegate<TItem, TValue, TValue, TState> updateValueFactory) where TItem : class
        {
            Should.NotBeNull(item, nameof(item));
            Should.NotBeNull(path, nameof(path));
            Should.NotBeNull(updateValueFactory, nameof(updateValueFactory));
            var dictionary = GetAttachedDictionary(item);
            lock (dictionary)
            {
                if (dictionary.TryGetValue(path, out var value))
                {
                    value = updateValueFactory(item, addValue, (TValue)value!, state);
                    dictionary[path] = value;
                    return (TValue)value!;
                }

                dictionary.Add(path, addValue);
                return addValue;
            }
        }

        public virtual TValue AddOrUpdate<TItem, TValue, TState>(TItem item, string path, TState state,
            Func<TItem, TState, TValue> addValueFactory, UpdateValueDelegate<TItem, Func<TItem, TState, TValue>, TValue, TState> updateValueFactory) where TItem : class
        {
            Should.NotBeNull(item, nameof(item));
            Should.NotBeNull(path, nameof(path));
            Should.NotBeNull(addValueFactory, nameof(addValueFactory));
            Should.NotBeNull(updateValueFactory, nameof(updateValueFactory));
            var dictionary = GetAttachedDictionary(item);
            lock (dictionary)
            {
                if (dictionary.TryGetValue(path, out var value))
                {
                    value = updateValueFactory(item, addValueFactory, (TValue)value!, state);
                    dictionary[path] = value;
                    return (TValue)value!;
                }

                value = addValueFactory(item, state);
                dictionary.Add(path, value);
                return (TValue)value!;
            }
        }

        public virtual TValue GetOrAdd<TItem, TValue>(TItem item, string path, TValue value) where TItem : class
        {
            Should.NotBeNull(item, nameof(item));
            Should.NotBeNull(path, nameof(path));
            var dictionary = GetAttachedDictionary(item);
            lock (dictionary)
            {
                if (dictionary.TryGetValue(path, out var oldValue))
                    return (TValue)oldValue!;
                dictionary.Add(path, value);
                return value;
            }
        }

        public virtual TValue GetOrAdd<TItem, TValue, TState>(TItem item, string path, TState state, Func<TItem, TState, TValue> valueFactory)
            where TItem : class
        {
            Should.NotBeNull(item, nameof(item));
            Should.NotBeNull(path, nameof(path));
            Should.NotBeNull(valueFactory, nameof(valueFactory));
            var dictionary = GetAttachedDictionary(item);
            lock (dictionary)
            {
                if (dictionary.TryGetValue(path, out var oldValue))
                    return (TValue)oldValue!;
                oldValue = valueFactory(item, state);
                dictionary.Add(path, oldValue);
                return (TValue)oldValue!;
            }
        }

        public virtual void SetValue<TItem, TValue>(TItem item, string path, TValue value) where TItem : class
        {
            Should.NotBeNull(item, nameof(item));
            Should.NotBeNull(path, nameof(path));
            var dictionary = GetAttachedDictionary(item);
            lock (dictionary)
            {
                dictionary[path] = value;
            }
        }

        public virtual bool Clear<TItem>(TItem item) where TItem : class
        {
            Should.NotBeNull(item, nameof(item));
            return ClearInternal(item);
        }

        public virtual bool Clear<TItem>(TItem item, string path) where TItem : class
        {
            var dictionary = GetAttachedDictionary(item);
            if (dictionary == null)
                return false;
            lock (dictionary)
            {
                if (dictionary.Remove(path))
                {
                    if (dictionary.Count == 0)
                        ClearInternal(item);
                    return true;
                }
            }

            return false;
        }

        #endregion

        #region Methods

        protected abstract LightDictionary<string, object?> GetAttachedDictionary<TItem>(TItem item) where TItem : class;

        protected abstract bool ClearInternal<TItem>(TItem item) where TItem : class;

        #endregion
    }
}