using System;
using System.Collections.Generic;
using MugenMvvm.Delegates;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Internal;

namespace MugenMvvm.Interfaces.Internal
{
    public interface IAttachedValueManager : IComponentOwner<IAttachedValueManager>
    {
        ItemOrList<KeyValuePair<string, object?>, IReadOnlyList<KeyValuePair<string, object?>>> GetValues(object item, Func<object, KeyValuePair<string, object?>, object?, bool>? predicate = null, object? state = null);

        bool TryGet(object item, string path, out object? value);

        bool Contains(object item, string path);

        TValue AddOrUpdate<TItem, TValue, TState>(TItem item, string path, TValue addValue, in TState state, UpdateValueDelegate<TItem, TValue, TValue, TState, TValue> updateValueFactory)
            where TItem : class;

        TValue AddOrUpdate<TItem, TValue, TState>(TItem item, string path, in TState state, Func<TItem, TState, TValue> addValueFactory, UpdateValueDelegate<TItem, TValue, TState, TValue> updateValueFactory)
            where TItem : class;

        TValue GetOrAdd<TItem, TValue, TState>(TItem item, string path, in TState state, Func<TItem, TState, TValue> valueFactory)
            where TItem : class;

        object? GetOrAdd(object item, string path, object? value);

        void Set(object item, string path, object? value, out object? oldValue);

        bool Clear(object item, string path, out object? oldValue);

        bool Clear(object item);
    }
}