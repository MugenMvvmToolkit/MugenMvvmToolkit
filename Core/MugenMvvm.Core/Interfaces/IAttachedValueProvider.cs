using System;
using System.Collections.Generic;
using MugenMvvm.Delegates;

namespace MugenMvvm.Interfaces
{
    public interface IAttachedValueProvider
    {
        TValue AddOrUpdate<TItem, TValue>(TItem item, string path, TValue addValue, UpdateValueDelegate<TItem, TValue, TValue, object?> updateValueFactory, object? state = null);

        TValue AddOrUpdate<TItem, TValue>(TItem item, string path,
            Func<TItem, object, TValue> addValueFactory, UpdateValueDelegate<TItem, Func<TItem, object?, TValue>, TValue, object> updateValueFactory, object? state = null);

        TValue GetOrAdd<TItem, TValue>(TItem item, string path, Func<TItem, object?, TValue> valueFactory, object? state);

        TValue GetOrAdd<TValue>(object item, string path, TValue value);

        bool TryGetValue<TValue>(object item, string path, out TValue value);

        void SetValue(object item, string path, object? value);

        bool Contains(object item, string path);

        IReadOnlyList<KeyValuePair<string, object?>> GetValues(object item, Func<string, object?, bool>? predicate);

        bool Clear(object item);

        bool Clear(object item, string path);
    }
}