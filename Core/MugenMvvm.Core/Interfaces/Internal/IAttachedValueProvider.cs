using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Delegates;

namespace MugenMvvm.Interfaces.Internal
{
    public interface IAttachedValueProvider
    {
        IReadOnlyList<KeyValuePair<string, object?>> GetValues<TItem, TState>(TItem item, TState state, Func<TItem, string, object?, TState, bool>? predicate = null)
            where TItem : class;

        bool TryGetValue<TItem, TValue>(TItem item, string path, [NotNullWhen(true)] out TValue value)
            where TItem : class;

        bool Contains<TItem>(TItem item, string path) where TItem : class;

        TValue AddOrUpdate<TItem, TValue, TState>(TItem item, string path, TValue addValue, TState state, UpdateValueDelegate<TItem, TValue, TValue, TState> updateValueFactory)
            where TItem : class;

        TValue AddOrUpdate<TItem, TValue, TState>(TItem item, string path, TState state1, Func<TItem, TState, TValue> addValueFactory,
            UpdateValueDelegate<TItem, Func<TItem, TState, TValue>, TValue, TState> updateValueFactory)
            where TItem : class;

        TValue GetOrAdd<TItem, TValue>(TItem item, string path, TValue value)
            where TItem : class;

        TValue GetOrAdd<TItem, TValue, TState>(TItem item, string path, TState state, Func<TItem, TState, TValue> valueFactory)
            where TItem : class;

        void SetValue<TItem, TValue>(TItem item, string path, TValue value)
            where TItem : class;

        bool Clear<TItem>(TItem item)
            where TItem : class;

        bool Clear<TItem>(TItem item, string path)
            where TItem : class;
    }
}