using System;
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

        public static TValue AddOrUpdate<TItem, TValue, TState1, TState2>(this IAttachedValueManager valueManager, TItem item, string path, TValue addValue, TState1 state1,
            TState2 state2,
            UpdateValueDelegate<TItem, TValue, TValue, TState1, TState2> updateValueFactory)
            where TItem : class
        {
            Should.NotBeNull(valueManager, nameof(valueManager));
            return valueManager.GetOrAddAttachedValueProvider(item).AddOrUpdate(item, path, addValue, state1, state2, updateValueFactory);
        }

        public static TValue AddOrUpdate<TItem, TValue, TState1, TState2>(this IAttachedValueManager valueManager, TItem item, string path, TState1 state1, TState2 state2,
            Func<TItem, TState1, TState2, TValue> addValueFactory, UpdateValueDelegate<TItem, Func<TItem, TState1, TState2, TValue>, TValue, TState1, TState2> updateValueFactory)
            where TItem : class
        {
            Should.NotBeNull(valueManager, nameof(valueManager));
            return valueManager.GetOrAddAttachedValueProvider(item).AddOrUpdate(item, path, state1, state2, addValueFactory, updateValueFactory);
        }

        public static TValue GetOrAdd<TItem, TValue>(this IAttachedValueManager valueManager, TItem item, string path, TValue value)
            where TItem : class
        {
            Should.NotBeNull(valueManager, nameof(valueManager));
            return valueManager.GetOrAddAttachedValueProvider(item).GetOrAdd(item, path, value);
        }

        public static TValue GetOrAdd<TItem, TValue, TState1, TState2>(this IAttachedValueManager valueManager, TItem item, string path, TState1 state1, TState2 state2,
            Func<TItem, TState1, TState2, TValue> valueFactory)
            where TItem : class
        {
            Should.NotBeNull(valueManager, nameof(valueManager));
            return valueManager.GetOrAddAttachedValueProvider(item).GetOrAdd(item, path, state1, state2, valueFactory);
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

        public static TValue GetOrAdd<TItem, TValue, TState1>(this IAttachedValueManager valueManager, TItem item, string path, TState1 state1,
            Func<TItem, TState1, TValue> valueFactory)
            where TItem : class
        {
            Should.NotBeNull(valueManager, nameof(valueManager));
            return valueManager.GetOrAddAttachedValueProvider(item).GetOrAdd(item, path, state1, valueFactory, (it, s1, s2) => s2(it, s1));
        }

        public static TValue GetOrAdd<TItem, TValue>(this IAttachedValueManager valueManager, TItem item, string path, Func<TItem, TValue> valueFactory)
            where TItem : class
        {
            Should.NotBeNull(valueManager, nameof(valueManager));
            return valueManager.GetOrAddAttachedValueProvider(item).GetOrAdd(item, path, valueFactory, valueFactory, (it, s1, _) => s1(it));
        }

        public static TValue AddOrUpdate<TItem, TValue>(this IAttachedValueManager valueManager, TItem item, string path, Func<TItem, TValue> addValueFactory,
            UpdateValueDelegate<TItem, Func<TItem, TValue>, TValue> updateValueFactory)
            where TItem : class
        {
            Should.NotBeNull(valueManager, nameof(valueManager));
            return valueManager.GetOrAddAttachedValueProvider(item)
                .AddOrUpdate(item, path, addValueFactory, updateValueFactory, (i, s1, _) => s1(i), (i, _, cV, s1, s2) => s2(i, s1, cV));
        }

        public static TValue AddOrUpdate<TItem, TValue, TState1>(this IAttachedValueManager valueManager, TItem item, string path, TValue addValue, TState1 state1,
            UpdateValueDelegate<TItem, TValue, TValue, TState1> updateValueFactory)
            where TItem : class
        {
            Should.NotBeNull(valueManager, nameof(valueManager));
            return valueManager.GetOrAddAttachedValueProvider(item)
                .AddOrUpdate(item, path, addValue, state1, updateValueFactory, (i, addV, cV, s1, s2) => s2(i, addV, cV, s1));
        }

        public static TValue AddOrUpdate<TItem, TValue>(this IAttachedValueManager valueManager, TItem item, string path, TValue addValue,
            UpdateValueDelegate<TItem, TValue, TValue> updateValueFactory)
            where TItem : class
        {
            Should.NotBeNull(valueManager, nameof(valueManager));
            return valueManager.GetOrAddAttachedValueProvider(item)
                .AddOrUpdate(item, path, addValue, updateValueFactory, updateValueFactory, (i, addV, cV, s1, _) => s1(i, addV, cV));
        }

        public static TValue GetOrAdd<TItem, TValue, TState1>(this IAttachedValueProvider valueProvider, TItem item, string path, TState1 state1,
            Func<TItem, TState1, TValue> valueFactory)
            where TItem : class
        {
            Should.NotBeNull(valueProvider, nameof(valueProvider));
            return valueProvider.GetOrAdd(item, path, state1, valueFactory, (it, s1, s2) => s2(it, s1));
        }

        public static TValue GetOrAdd<TItem, TValue>(this IAttachedValueProvider valueProvider, TItem item, string path, Func<TItem, TValue> valueFactory)
            where TItem : class
        {
            Should.NotBeNull(valueProvider, nameof(valueProvider));
            return valueProvider.GetOrAdd(item, path, valueFactory, valueFactory, (it, s1, _) => s1(it));
        }

        public static TValue AddOrUpdate<TItem, TValue>(this IAttachedValueProvider valueProvider, TItem item, string path, Func<TItem, TValue> addValueFactory,
            UpdateValueDelegate<TItem, Func<TItem, TValue>, TValue> updateValueFactory)
            where TItem : class
        {
            Should.NotBeNull(valueProvider, nameof(valueProvider));
            return valueProvider.AddOrUpdate(item, path, addValueFactory, updateValueFactory, (i, s1, _) => s1(i), (i, _, cV, s1, s2) => s2(i, s1, cV));
        }

        public static TValue AddOrUpdate<TItem, TValue, TState1>(this IAttachedValueProvider valueProvider, TItem item, string path, TValue addValue, TState1 state1,
            UpdateValueDelegate<TItem, TValue, TValue, TState1> updateValueFactory)
            where TItem : class
        {
            Should.NotBeNull(valueProvider, nameof(valueProvider));
            return valueProvider.AddOrUpdate(item, path, addValue, state1, updateValueFactory, (i, addV, cV, s1, s2) => s2(i, addV, cV, s1));
        }

        public static TValue AddOrUpdate<TItem, TValue>(this IAttachedValueProvider valueProvider, TItem item, string path, TValue addValue,
            UpdateValueDelegate<TItem, TValue, TValue> updateValueFactory)
            where TItem : class
        {
            Should.NotBeNull(valueProvider, nameof(valueProvider));
            return valueProvider.AddOrUpdate(item, path, addValue, updateValueFactory, updateValueFactory, (i, addV, cV, s1, _) => s1(i, addV, cV));
        }

        #endregion
    }
}