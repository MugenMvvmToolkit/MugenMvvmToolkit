using System;
using System.Collections.Generic;
using MugenMvvm.Delegates;
using MugenMvvm.Internal;

namespace MugenMvvm.Interfaces.Internal
{
    public interface IAttachedValueStorageManager
    {
        int GetCount(object item, ref object? internalState);

        ItemOrList<KeyValuePair<string, object?>, IReadOnlyList<KeyValuePair<string, object?>>> GetValues<TState>(object item, ref object? internalState,
            TState state = default, Func<object, KeyValuePair<string, object?>, TState, bool>? predicate = null);

        bool Contains(object item, ref object? internalState, string path);

        bool TryGet(object item, ref object? internalState, string path, out object? value);

        TValue AddOrUpdate<TValue, TState>(object item, ref object? internalState, string path, TValue addValue, TState state, UpdateValueDelegate<object, TValue, TValue, TState, TValue> updateValueFactory);

        TValue AddOrUpdate<TValue, TState>(object item, ref object? internalState, string path, TState state, Func<object, TState, TValue> addValueFactory, UpdateValueDelegate<object, TValue, TState, TValue> updateValueFactory);

        TValue GetOrAdd<TValue, TState>(object item, ref object? internalState, string path, TState state, Func<object, TState, TValue> valueFactory);

        TValue GetOrAdd<TValue>(object item, ref object? internalState, string path, TValue value);

        void Set(object item, ref object? internalState, string path, object? value, out object? oldValue);

        bool Remove(object item, ref object? internalState, string path, out object? oldValue);

        bool Clear(object item, ref object? internalState);
    }
}