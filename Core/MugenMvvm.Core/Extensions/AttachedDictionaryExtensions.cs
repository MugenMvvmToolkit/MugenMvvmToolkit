using System;
using System.Collections.Generic;
using MugenMvvm.Delegates;
using MugenMvvm.Interfaces.Internal;

// ReSharper disable once CheckNamespace
namespace MugenMvvm
{
    public static partial class MugenExtensions
    {
        #region Methods

        public static IReadOnlyList<KeyValuePair<string, object?>> GetValues<TItem>(this IAttachedDictionaryProvider provider, TItem item, Func<TItem, string, object?, bool>? predicate)
            where TItem : class
        {
            Should.NotBeNull(provider, nameof(provider));
            var dictionary = provider.GetAttachedDictionary(item);
            if (dictionary == null)
                return Default.EmptyArray<KeyValuePair<string, object?>>();
            return dictionary.GetValues(item, predicate);
        }

        public static bool TryGetValue<TItem, TValue>(this IAttachedDictionaryProvider provider, TItem item, string path, out TValue value)
            where TItem : class
        {
            Should.NotBeNull(provider, nameof(provider));
            var dictionary = provider.GetAttachedDictionary(item);
            if (dictionary != null)
                return dictionary.TryGetValue(item, path, out value);
            value = default!;
            return false;
        }

        public static bool Contains<TItem>(this IAttachedDictionaryProvider provider, TItem item, string path)
            where TItem : class
        {
            Should.NotBeNull(provider, nameof(provider));
            var dictionary = provider.GetAttachedDictionary(item);
            return dictionary != null && dictionary.Contains(item, path);
        }

        public static TValue AddOrUpdate<TItem, TValue, TState1, TState2>(this IAttachedDictionaryProvider provider, TItem item, string path, TValue addValue, TState1 state1,
            TState2 state2,
            UpdateValueDelegate<TItem, TValue, TValue, TState1, TState2> updateValueFactory)
            where TItem : class
        {
            Should.NotBeNull(provider, nameof(provider));
            return provider.GetOrAddAttachedDictionary(item).AddOrUpdate(item, path, addValue, state1, state2, updateValueFactory);
        }

        public static TValue AddOrUpdate<TItem, TValue, TState1, TState2>(this IAttachedDictionaryProvider provider, TItem item, string path, TState1 state1, TState2 state2,
            Func<TItem, TState1, TState2, TValue> addValueFactory, UpdateValueDelegate<TItem, Func<TItem, TState1, TState2, TValue>, TValue, TState1, TState2> updateValueFactory)
            where TItem : class
        {
            Should.NotBeNull(provider, nameof(provider));
            return provider.GetOrAddAttachedDictionary(item).AddOrUpdate(item, path, state1, state2, addValueFactory, updateValueFactory);
        }

        public static TValue GetOrAdd<TItem, TValue>(this IAttachedDictionaryProvider provider, TItem item, string path, TValue value)
            where TItem : class
        {
            Should.NotBeNull(provider, nameof(provider));
            return provider.GetOrAddAttachedDictionary(item).GetOrAdd(item, path, value);
        }

        public static TValue GetOrAdd<TItem, TValue, TState1, TState2>(this IAttachedDictionaryProvider provider, TItem item, string path, TState1 state1, TState2 state2,
            Func<TItem, TState1, TState2, TValue> valueFactory)
            where TItem : class
        {
            Should.NotBeNull(provider, nameof(provider));
            return provider.GetOrAddAttachedDictionary(item).GetOrAdd(item, path, state1, state2, valueFactory);
        }

        public static void SetValue<TItem, TValue>(this IAttachedDictionaryProvider provider, TItem item, string path, TValue value)
            where TItem : class
        {
            Should.NotBeNull(provider, nameof(provider));
            provider.GetOrAddAttachedDictionary(item).SetValue(item, path, value);
        }

        public static bool Clear<TItem>(this IAttachedDictionaryProvider provider, TItem item, string path) where TItem : class
        {
            Should.NotBeNull(provider, nameof(provider));
            var dictionary = provider.GetAttachedDictionary(item);
            if (dictionary == null)
                return false;
            return dictionary.Clear(item, path);
        }

        public static bool Clear<TItem>(this IAttachedDictionaryProvider provider, TItem item) where TItem : class
        {
            Should.NotBeNull(provider, nameof(provider));
            var dictionary = provider.GetAttachedDictionary(item);
            if (dictionary == null)
                return false;
            return dictionary.Clear(item);
        }

        public static TValue GetOrAdd<TItem, TValue, TState1>(this IAttachedDictionaryProvider provider, TItem item, string path, TState1 state1,
            Func<TItem, TState1, TValue> valueFactory)
            where TItem : class
        {
            Should.NotBeNull(provider, nameof(provider));
            return provider.GetOrAddAttachedDictionary(item).GetOrAdd(item, path, state1, valueFactory, (it, s1, s2) => s2(it, s1));
        }

        public static TValue GetOrAdd<TItem, TValue>(this IAttachedDictionaryProvider provider, TItem item, string path, Func<TItem, TValue> valueFactory)
            where TItem : class
        {
            Should.NotBeNull(provider, nameof(provider));
            return provider.GetOrAddAttachedDictionary(item).GetOrAdd(item, path, valueFactory, valueFactory, (it, s1, _) => s1(it));
        }

        public static TValue AddOrUpdate<TItem, TValue>(this IAttachedDictionaryProvider provider, TItem item, string path, Func<TItem, TValue> addValueFactory,
            UpdateValueDelegate<TItem, Func<TItem, TValue>, TValue> updateValueFactory)
            where TItem : class
        {
            Should.NotBeNull(provider, nameof(provider));
            return provider.GetOrAddAttachedDictionary(item)
                .AddOrUpdate(item, path, addValueFactory, updateValueFactory, (i, s1, _) => s1(i), (i, _, cV, s1, s2) => s2(i, s1, cV));
        }

        public static TValue AddOrUpdate<TItem, TValue, TState1>(this IAttachedDictionaryProvider provider, TItem item, string path, TValue addValue, TState1 state1,
            UpdateValueDelegate<TItem, TValue, TValue, TState1> updateValueFactory)
            where TItem : class
        {
            Should.NotBeNull(provider, nameof(provider));
            return provider.GetOrAddAttachedDictionary(item)
                .AddOrUpdate(item, path, addValue, state1, updateValueFactory, (i, addV, cV, s1, s2) => s2(i, addV, cV, s1));
        }

        public static TValue AddOrUpdate<TItem, TValue>(this IAttachedDictionaryProvider provider, TItem item, string path, TValue addValue,
            UpdateValueDelegate<TItem, TValue, TValue> updateValueFactory)
            where TItem : class
        {
            Should.NotBeNull(provider, nameof(provider));
            return provider.GetOrAddAttachedDictionary(item)
                .AddOrUpdate(item, path, addValue, updateValueFactory, updateValueFactory, (i, addV, cV, s1, _) => s1(i, addV, cV));
        }

        public static TValue GetOrAdd<TItem, TValue, TState1>(this IAttachedDictionary dictionary, TItem item, string path, TState1 state1,
            Func<TItem, TState1, TValue> valueFactory)
            where TItem : class
        {
            Should.NotBeNull(dictionary, nameof(dictionary));
            return dictionary.GetOrAdd(item, path, state1, valueFactory, (it, s1, s2) => s2(it, s1));
        }

        public static TValue GetOrAdd<TItem, TValue>(this IAttachedDictionary dictionary, TItem item, string path, Func<TItem, TValue> valueFactory)
            where TItem : class
        {
            Should.NotBeNull(dictionary, nameof(dictionary));
            return dictionary.GetOrAdd(item, path, valueFactory, valueFactory, (it, s1, _) => s1(it));
        }

        public static TValue AddOrUpdate<TItem, TValue>(this IAttachedDictionary dictionary, TItem item, string path, Func<TItem, TValue> addValueFactory,
            UpdateValueDelegate<TItem, Func<TItem, TValue>, TValue> updateValueFactory)
            where TItem : class
        {
            Should.NotBeNull(dictionary, nameof(dictionary));
            return dictionary.AddOrUpdate(item, path, addValueFactory, updateValueFactory, (i, s1, _) => s1(i), (i, _, cV, s1, s2) => s2(i, s1, cV));
        }

        public static TValue AddOrUpdate<TItem, TValue, TState1>(this IAttachedDictionary dictionary, TItem item, string path, TValue addValue, TState1 state1,
            UpdateValueDelegate<TItem, TValue, TValue, TState1> updateValueFactory)
            where TItem : class
        {
            Should.NotBeNull(dictionary, nameof(dictionary));
            return dictionary.AddOrUpdate(item, path, addValue, state1, updateValueFactory, (i, addV, cV, s1, s2) => s2(i, addV, cV, s1));
        }

        public static TValue AddOrUpdate<TItem, TValue>(this IAttachedDictionary dictionary, TItem item, string path, TValue addValue,
            UpdateValueDelegate<TItem, TValue, TValue> updateValueFactory)
            where TItem : class
        {
            Should.NotBeNull(dictionary, nameof(dictionary));
            return dictionary.AddOrUpdate(item, path, addValue, updateValueFactory, updateValueFactory, (i, addV, cV, s1, _) => s1(i, addV, cV));
        }

        #endregion
    }
}