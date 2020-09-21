using System;
using System.Collections.Generic;
using MugenMvvm.Internal;

namespace MugenMvvm.Interfaces.Internal
{
    public interface IAttachedValueStorageManager
    {
        int GetCount(object item, ref object? internalState);

        ItemOrList<KeyValuePair<string, object?>, IReadOnlyList<KeyValuePair<string, object?>>> GetValues<TState>(object item, TState state,
            Func<object, string, object?, TState, bool>? predicate, ref object? internalState);

        bool Contains(object item, string path, ref object? internalState);

        bool TryGet(object item, string path, ref object? internalState, out object? value);

        TValue AddOrUpdate<TValue, TState>(object item, string path, TValue addValue, TState state, Func<object, string, TValue, TState, TValue> updateValueFactory, ref object? internalState);

        TValue AddOrUpdate<TValue, TState>(object item, string path, TState state, Func<object, TState, TValue> addValueFactory,
            Func<object, string, TValue, TState, TValue> updateValueFactory, ref object? internalState);

        TValue GetOrAdd<TValue, TState>(object item, string path, TState state, Func<object, TState, TValue> valueFactory, ref object? internalState);

        TValue GetOrAdd<TValue>(object item, string path, TValue value, ref object? internalState);

        void Set(object item, string path, object? value, ref object? internalState, out object? oldValue);

        bool Remove(object item, string path, ref object? internalState, out object? oldValue);

        bool Clear(object item, ref object? internalState);
    }
}