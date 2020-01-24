using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Collections;
using MugenMvvm.Delegates;
using MugenMvvm.Interfaces.Internal;

namespace MugenMvvm.Extensions
{
    public static partial class MugenExtensions
    {
        #region Methods

        public static IReadOnlyList<KeyValuePair<string, object?>> GetValues<TItem, TState>(
            this LightDictionary<string, object?> dictionary, TItem item, TState state, Func<TItem, KeyValuePair<string, object?>, TState, bool>? predicate = null)
            where TItem : class
        {
            Should.NotBeNull(dictionary, nameof(dictionary));
            Should.NotBeNull(item, nameof(item));
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

        public static bool TryGetValue<TValue>(this LightDictionary<string, object?> dictionary, string path, [NotNullWhen(true)] out TValue value)
        {
            Should.NotBeNull(dictionary, nameof(dictionary));
            Should.NotBeNull(path, nameof(path));
            if (dictionary.TryGetValue(path, out var result))
            {
                value = (TValue)result!;
                return true;
            }

            value = default!;
            return false;
        }

        public static TValue AddOrUpdate<TItem, TValue, TState>(this LightDictionary<string, object?> dictionary, TItem item, string path, TValue addValue, TState state, UpdateValueDelegate<TItem, TValue, TValue, TState> updateValueFactory) where TItem : class
        {
            Should.NotBeNull(dictionary, nameof(dictionary));
            Should.NotBeNull(item, nameof(item));
            Should.NotBeNull(path, nameof(path));
            Should.NotBeNull(updateValueFactory, nameof(updateValueFactory));
            if (dictionary.TryGetValue(path, out var value))
            {
                value = updateValueFactory(item, addValue, (TValue)value!, state);
                dictionary[path] = value;
                return (TValue)value!;
            }

            dictionary.Add(path, addValue);
            return addValue;
        }

        public static TValue AddOrUpdate<TItem, TValue, TState>(this LightDictionary<string, object?> dictionary, TItem item, string path, TState state,
            Func<TItem, TState, TValue> addValueFactory, UpdateValueDelegate<TItem, Func<TItem, TState, TValue>, TValue, TState> updateValueFactory) where TItem : class
        {
            Should.NotBeNull(dictionary, nameof(dictionary));
            Should.NotBeNull(item, nameof(item));
            Should.NotBeNull(path, nameof(path));
            Should.NotBeNull(addValueFactory, nameof(addValueFactory));
            Should.NotBeNull(updateValueFactory, nameof(updateValueFactory));
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

        public static TValue GetOrAdd<TValue>(this LightDictionary<string, object?> dictionary, string path, TValue value)
        {
            Should.NotBeNull(dictionary, nameof(dictionary));
            Should.NotBeNull(path, nameof(path));
            if (dictionary.TryGetValue(path, out var oldValue))
                return (TValue)oldValue!;
            dictionary.Add(path, value);
            return value;
        }

        public static TValue GetOrAdd<TItem, TValue, TState>(this LightDictionary<string, object?> dictionary, TItem item, string path, TState state, Func<TItem, TState, TValue> valueFactory)
            where TItem : class
        {
            Should.NotBeNull(dictionary, nameof(dictionary));
            Should.NotBeNull(item, nameof(item));
            Should.NotBeNull(path, nameof(path));
            Should.NotBeNull(valueFactory, nameof(valueFactory));
            if (dictionary.TryGetValue(path, out var oldValue))
                return (TValue)oldValue!;
            oldValue = valueFactory(item, state);
            dictionary.Add(path, oldValue);
            return (TValue)oldValue!;
        }

        public static bool TryGet<TValue>(this IAttachedValueManager valueManager, object item, string path, [NotNullWhen(true)] out TValue value)
        {
            Should.NotBeNull(valueManager, nameof(valueManager));
            var provider = valueManager.GetAttachedValueProvider(item);
            if (provider != null)
                return provider.TryGet(item, path, out value);
            value = default!;
            return false;
        }

        public static bool Contains(this IAttachedValueManager valueManager, object item, string path)
        {
            Should.NotBeNull(valueManager, nameof(valueManager));
            var provider = valueManager.GetAttachedValueProvider(item);
            return provider != null && provider.Contains(item, path);
        }

        [return: NotNullIfNotNull("addValue")]
        public static TValue AddOrUpdate<TItem, TValue, TState>(this IAttachedValueManager valueManager, TItem item, string path, TValue addValue, TState state,
            UpdateValueDelegate<TItem, TValue, TValue, TState> updateValueFactory)
            where TItem : class
        {
            Should.NotBeNull(valueManager, nameof(valueManager));
            return valueManager.GetOrAddAttachedValueProvider(item).AddOrUpdate(item, path, addValue, state, updateValueFactory);
        }

        public static TValue AddOrUpdate<TItem, TValue, TState>(this IAttachedValueManager valueManager, TItem item, string path, TState state,
            Func<TItem, TState, TValue> addValueFactory, UpdateValueDelegate<TItem, Func<TItem, TState, TValue>, TValue, TState> updateValueFactory)
            where TItem : class
        {
            Should.NotBeNull(valueManager, nameof(valueManager));
            return valueManager.GetOrAddAttachedValueProvider(item).AddOrUpdate(item, path, state, addValueFactory, updateValueFactory);
        }

        [return: NotNullIfNotNull("value")]
        public static TValue GetOrAdd<TValue>(this IAttachedValueManager valueManager, object item, string path, TValue value)
        {
            Should.NotBeNull(valueManager, nameof(valueManager));
            return valueManager.GetOrAddAttachedValueProvider(item).GetOrAdd(item, path, value);
        }

        public static TValue GetOrAdd<TItem, TValue, TState>(this IAttachedValueManager valueManager, TItem item, string path, TState state,
            Func<TItem, TState, TValue> valueFactory)
            where TItem : class
        {
            Should.NotBeNull(valueManager, nameof(valueManager));
            return valueManager.GetOrAddAttachedValueProvider(item).GetOrAdd(item, path, state, valueFactory);
        }

        public static TValue GetOrAdd<TItem, TValue>(this IAttachedValueManager valueManager, TItem item, string path, Func<TItem, TValue> valueFactory)
            where TItem : class
        {
            Should.NotBeNull(valueManager, nameof(valueManager));
            return valueManager.GetOrAddAttachedValueProvider(item).GetOrAdd(item, path, valueFactory: valueFactory);
        }

        public static void Set<TValue>(this IAttachedValueManager valueManager, object item, string path, TValue value)
        {
            Should.NotBeNull(valueManager, nameof(valueManager));
            valueManager.GetOrAddAttachedValueProvider(item).Set(item, path, value);
        }

        public static bool Clear(this IAttachedValueManager valueManager, object item, string path)
        {
            Should.NotBeNull(valueManager, nameof(valueManager));
            var provider = valueManager.GetAttachedValueProvider(item);
            if (provider == null)
                return false;
            return provider.Clear(item, path);
        }

        public static bool Clear(this IAttachedValueManager valueManager, object item)
        {
            Should.NotBeNull(valueManager, nameof(valueManager));
            var provider = valueManager.GetAttachedValueProvider(item);
            if (provider == null)
                return false;
            return provider.Clear(item);
        }

        public static TValue GetOrAdd<TItem, TValue>(this IAttachedValueProvider valueProvider, TItem item, string path, Func<TItem, TValue> valueFactory)
            where TItem : class
        {
            Should.NotBeNull(valueProvider, nameof(valueProvider));
            return valueProvider.GetOrAdd(item, path, valueFactory, (it, s) => s(it));
        }

        #endregion
    }
}