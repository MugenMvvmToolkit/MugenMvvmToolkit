using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Delegates;

namespace MugenMvvm.Interfaces.Internal
{
    public interface IAttachedValueProvider
    {
        IReadOnlyList<KeyValuePair<string, object?>> GetValues<TItem, TState>(TItem item, TState state, Func<TItem, string, TState, bool>? predicate = null)
            where TItem : class;

        bool TryGetValue<TItem, TValue>(TItem item, string path, [NotNullWhen(true)] out TValue value)
            where TItem : class;

        bool Contains<TItem>(TItem item, string path);

        TValue AddOrUpdate<TItem, TValue, TState1, TState2>(TItem item, string path, TValue addValue, TState1 state1, TState2 state2,
            UpdateValueDelegate<TItem, TValue, TValue, TState1, TState2> updateValueFactory)
            where TItem : class;

        TValue AddOrUpdate<TItem, TValue, TState1, TState2>(TItem item, string path, TState1 state1, TState2 state2, Func<TItem, TState1, TState2, TValue> addValueFactory,
            UpdateValueDelegate<TItem, Func<TItem, TState1, TState2, TValue>, TValue, TState1, TState2> updateValueFactory)
            where TItem : class;

        TValue GetOrAdd<TItem, TValue>(TItem item, string path, TValue value)
            where TItem : class;

        TValue GetOrAdd<TItem, TValue, TState1, TState2>(TItem item, string path, TState1 state1, TState2 state2, Func<TItem, TState1, TState2, TValue> valueFactory)
            where TItem : class;

        void SetValue<TItem, TValue>(TItem item, string path, TValue value)
            where TItem : class;

        bool Clear<TItem>(TItem item)
            where TItem : class;

        bool Clear<TItem>(TItem item, string path)
            where TItem : class;
    }
}