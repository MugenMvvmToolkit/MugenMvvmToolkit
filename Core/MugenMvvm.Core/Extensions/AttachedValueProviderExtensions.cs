using System;
using MugenMvvm.Delegates;
using MugenMvvm.Interfaces.Internal;

// ReSharper disable once CheckNamespace
namespace MugenMvvm
{
    public static partial class MugenExtensions
    {
        #region Methods

        public static TValue GetOrAdd<TItem, TValue, TState1>(this IAttachedValueProvider attachedValueProvider, TItem item, string path, TState1 state1,
            Func<TItem, TState1, TValue> valueFactory)
            where TItem : class
        {
            Should.NotBeNull(attachedValueProvider, nameof(attachedValueProvider));
            return attachedValueProvider.GetOrAdd(item, path, state1, valueFactory, (it, s1, s2) => s2(it, s1));
        }

        public static TValue GetOrAdd<TItem, TValue>(this IAttachedValueProvider attachedValueProvider, TItem item, string path, Func<TItem, TValue> valueFactory)
            where TItem : class
        {
            Should.NotBeNull(attachedValueProvider, nameof(attachedValueProvider));
            return attachedValueProvider.GetOrAdd(item, path, valueFactory, valueFactory, (it, s1, _) => s1(it));
        }

        public static TValue AddOrUpdate<TItem, TValue>(this IAttachedValueProvider attachedValueProvider, TItem item, string path, Func<TItem, TValue> addValueFactory,
            UpdateValueDelegate<TItem, Func<TItem, TValue>, TValue> updateValueFactory)
            where TItem : class
        {
            Should.NotBeNull(attachedValueProvider, nameof(attachedValueProvider));
            return attachedValueProvider.AddOrUpdate(item, path, addValueFactory, updateValueFactory, (i, s1, _) => s1(i),
                (i, _, currentValue, s1, s2) => s2(i, s1, currentValue));
        }

        public static TValue AddOrUpdate<TItem, TValue, TState1>(this IAttachedValueProvider attachedValueProvider, TItem item, string path, TValue addValue, TState1 state1,
            UpdateValueDelegate<TItem, TValue, TValue, TState1> updateValueFactory)
            where TItem : class
        {
            Should.NotBeNull(attachedValueProvider, nameof(attachedValueProvider));
            return attachedValueProvider.AddOrUpdate(item, path, addValue, state1, updateValueFactory, (i, addV, cV, s1, s2) => s2(i, addV, cV, s1));
        }

        public static TValue AddOrUpdate<TItem, TValue>(this IAttachedValueProvider attachedValueProvider, TItem item, string path, TValue addValue,
            UpdateValueDelegate<TItem, TValue, TValue> updateValueFactory)
            where TItem : class
        {
            Should.NotBeNull(attachedValueProvider, nameof(attachedValueProvider));
            return attachedValueProvider.AddOrUpdate(item, path, addValue, updateValueFactory, updateValueFactory, (i, addV, cV, s1, _) => s1(i, addV, cV));
        }

        #endregion
    }
}