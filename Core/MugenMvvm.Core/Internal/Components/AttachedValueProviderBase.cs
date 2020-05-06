using System;
using System.Collections.Generic;
using MugenMvvm.Collections;
using MugenMvvm.Delegates;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Internal.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Internal.Components
{
    public abstract class AttachedValueProviderBase : IAttachedValueProviderComponent
    {
        #region Implementation of interfaces

        public abstract bool IsSupported(object item, IReadOnlyMetadataContext? metadata);

        public virtual IReadOnlyList<KeyValuePair<string, object?>>? TryGetValues<TItem, TState>(TItem item, TState state, Func<TItem, KeyValuePair<string, object?>, TState, bool>? predicate) where TItem : class
        {
            Should.NotBeNull(item, nameof(item));
            var dictionary = GetAttachedDictionary(item, true);
            if (dictionary == null)
                return Default.EmptyArray<KeyValuePair<string, object?>>();
            lock (dictionary)
            {
                if (predicate == null)
                    return new List<KeyValuePair<string, object?>>(dictionary);
                var list = new List<KeyValuePair<string, object?>>();
                foreach (var keyValue in dictionary)
                {
                    if (predicate(item, keyValue, state))
                        list.Add(keyValue);
                }

                return list;
            }
        }

        public virtual bool TryGet<TValue>(object item, string path, out TValue value)
        {
            Should.NotBeNull(item, nameof(item));
            Should.NotBeNull(path, nameof(path));
            var dictionary = GetAttachedDictionary(item, true);
            if (dictionary == null)
            {
                value = default!;
                return false;
            }

            lock (dictionary)
            {
                if (dictionary.TryGetValue(path, out var result))
                {
                    value = (TValue) result!;
                    return true;
                }

                value = default!;
                return false;
            }
        }

        public virtual bool Contains(object item, string path)
        {
            Should.NotBeNull(item, nameof(item));
            var dictionary = GetAttachedDictionary(item, true);
            if (dictionary == null)
                return false;
            lock (dictionary)
            {
                return dictionary.ContainsKey(path);
            }
        }

        public virtual TValue AddOrUpdate<TItem, TValue, TState>(TItem item, string path, TValue addValue, TState state, UpdateValueDelegate<TItem, TValue, TValue, TState, TValue> updateValueFactory) where TItem : class
        {
            Should.NotBeNull(item, nameof(item));
            Should.NotBeNull(path, nameof(path));
            Should.NotBeNull(updateValueFactory, nameof(updateValueFactory));
            var dictionary = GetAttachedDictionary(item, false)!;
            lock (dictionary)
            {
                if (dictionary.TryGetValue(path, out var value))
                {
                    value = BoxingExtensions.Box(updateValueFactory(item, addValue, (TValue) value!, state));
                    dictionary[path] = value;
                    return (TValue) value!;
                }

                dictionary.Add(path, BoxingExtensions.Box(addValue));
                return addValue;
            }
        }

        public virtual TValue AddOrUpdate<TItem, TValue, TState>(TItem item, string path, TState state, Func<TItem, TState, TValue> addValueFactory, 
            UpdateValueDelegate<TItem, TValue, TState, TValue> updateValueFactory) where TItem : class
        {
            Should.NotBeNull(item, nameof(item));
            Should.NotBeNull(path, nameof(path));
            Should.NotBeNull(addValueFactory, nameof(addValueFactory));
            Should.NotBeNull(updateValueFactory, nameof(updateValueFactory));
            var dictionary = GetAttachedDictionary(item, false)!;
            lock (dictionary)
            {
                if (dictionary.TryGetValue(path, out var value))
                {
                    value = BoxingExtensions.Box(updateValueFactory(item, addValueFactory, (TValue) value!, state));
                    dictionary[path] = value;
                    return (TValue) value!;
                }

                value = BoxingExtensions.Box(addValueFactory(item, state));
                dictionary.Add(path, value);
                return (TValue) value!;
            }
        }

        public virtual TValue GetOrAdd<TValue>(object item, string path, TValue value)
        {
            Should.NotBeNull(item, nameof(item));
            Should.NotBeNull(path, nameof(path));
            var dictionary = GetAttachedDictionary(item, false)!;
            lock (dictionary)
            {
                if (dictionary.TryGetValue(path, out var oldValue))
                    return (TValue) oldValue!;
                dictionary.Add(path, BoxingExtensions.Box(value));
                return value;
            }
        }

        public virtual TValue GetOrAdd<TItem, TValue, TState>(TItem item, string path, TState state, Func<TItem, TState, TValue> valueFactory) where TItem : class
        {
            Should.NotBeNull(item, nameof(item));
            Should.NotBeNull(path, nameof(path));
            Should.NotBeNull(valueFactory, nameof(valueFactory));
            var dictionary = GetAttachedDictionary(item, false)!;
            lock (dictionary)
            {
                if (dictionary.TryGetValue(path, out var oldValue))
                    return (TValue) oldValue!;
                oldValue = BoxingExtensions.Box(valueFactory(item, state));
                dictionary.Add(path, oldValue);
                return (TValue) oldValue!;
            }
        }

        public virtual void Set<TValue>(object item, string path, TValue value)
        {
            Should.NotBeNull(item, nameof(item));
            Should.NotBeNull(path, nameof(path));
            var dictionary = GetAttachedDictionary(item, false)!;
            lock (dictionary)
            {
                dictionary[path] = BoxingExtensions.Box(value);
            }
        }

        public virtual bool Clear(object item, string? path)
        {
            if (string.IsNullOrEmpty(path))
                return ClearInternal(item);

            var dictionary = GetAttachedDictionary(item, true);
            if (dictionary == null)
                return false;

            bool clear;
            bool removed;
            lock (dictionary)
            {
                removed = dictionary.Remove(path!);
                clear = removed && dictionary.Count == 0;
            }

            if (clear)
                return ClearInternal(item);
            return removed;
        }

        #endregion

        #region Methods

        protected abstract LightDictionary<string, object?>? GetAttachedDictionary(object item, bool optional);

        protected abstract bool ClearInternal(object item);

        #endregion
    }
}