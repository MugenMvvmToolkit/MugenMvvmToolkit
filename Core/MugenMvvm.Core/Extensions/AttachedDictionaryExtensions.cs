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

        public static bool TryGetValue<TItem, TValue>(this IAttachedValueManager valueManager, TItem item, string path, out TValue value)
            where TItem : class
        {
            Should.NotBeNull(valueManager, nameof(valueManager));
            var provider = valueManager.GetAttachedValueProvider(item);
            if (provider != null)
                return provider.TryGetValue(item, path, out value);
            value = default!;
            return false;
        }

        public static bool Contains<TItem>(this IAttachedValueManager valueManager, TItem item, string path)
            where TItem : class
        {
            Should.NotBeNull(valueManager, nameof(valueManager));
            var provider = valueManager.GetAttachedValueProvider(item);
            return provider != null && provider.Contains(item, path);
        }

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

        public static TValue AddOrUpdate<TItem, TValue>(this IAttachedValueManager valueManager, TItem item, string path, Func<TItem, TValue> addValueFactory,
            UpdateValueDelegate<TItem, Func<TItem, TValue>, TValue> updateValueFactory)
            where TItem : class
        {
            Should.NotBeNull(valueManager, nameof(valueManager));
            return valueManager
                .GetOrAddAttachedValueProvider(item)
                .AddOrUpdate(item, path, addValueFactory, updateValueFactory);
        }

        public static TValue AddOrUpdate<TItem, TValue>(this IAttachedValueManager valueManager, TItem item, string path, TValue addValue,
            UpdateValueDelegate<TItem, TValue, TValue> updateValueFactory)
            where TItem : class
        {
            Should.NotBeNull(valueManager, nameof(valueManager));
            return valueManager
                .GetOrAddAttachedValueProvider(item)
                .AddOrUpdate(item, path, addValue, updateValueFactory);
        }

        public static TValue GetOrAdd<TItem, TValue>(this IAttachedValueManager valueManager, TItem item, string path, TValue value)
            where TItem : class
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

        public static void SetValue<TItem, TValue>(this IAttachedValueManager valueManager, TItem item, string path, TValue value)
            where TItem : class
        {
            Should.NotBeNull(valueManager, nameof(valueManager));
            valueManager.GetOrAddAttachedValueProvider(item).SetValue(item, path, value);
        }

        public static bool Clear<TItem>(this IAttachedValueManager valueManager, TItem item, string path) where TItem : class
        {
            Should.NotBeNull(valueManager, nameof(valueManager));
            var provider = valueManager.GetAttachedValueProvider(item);
            if (provider == null)
                return false;
            return provider.Clear(item, path);
        }

        public static bool Clear<TItem>(this IAttachedValueManager valueManager, TItem item) where TItem : class
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

        public static TValue AddOrUpdate<TItem, TValue>(this IAttachedValueProvider valueProvider, TItem item, string path, Func<TItem, TValue> addValueFactory,
            UpdateValueDelegate<TItem, Func<TItem, TValue>, TValue> updateValueFactory)
            where TItem : class
        {
            Should.NotBeNull(valueProvider, nameof(valueProvider));
            var pair = new KeyValuePair<Func<TItem, TValue>, UpdateValueDelegate<TItem, Func<TItem, TValue>, TValue>>(addValueFactory, updateValueFactory);
            return valueProvider.AddOrUpdate(item, path, pair, (i, s1) => s1.Key(i),
                (i, _, cV, s) => s.Value(i, s.Key, cV));
        }

        public static TValue AddOrUpdate<TItem, TValue>(this IAttachedValueProvider valueProvider, TItem item, string path, TValue addValue,
            UpdateValueDelegate<TItem, TValue, TValue> updateValueFactory)
            where TItem : class
        {
            Should.NotBeNull(valueProvider, nameof(valueProvider));
            return valueProvider.AddOrUpdate(item, path, addValue, updateValueFactory, (i, addV, cV, s) => s(i, addV, cV));
        }

        #endregion
    }
}