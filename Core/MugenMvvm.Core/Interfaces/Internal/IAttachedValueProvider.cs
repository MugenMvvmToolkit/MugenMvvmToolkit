using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Delegates;
using MugenMvvm.Interfaces.App;
using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Interfaces.Internal
{
    public interface IAttachedValueProvider : IComponentOwner<IAttachedValueProvider>, IComponent<IMugenApplication>
    {
        IReadOnlyList<KeyValuePair<string, object?>> GetValues<TItem, TState>(TItem item, TState state, Func<TItem, KeyValuePair<string, object?>, TState, bool>? predicate = null)
            where TItem : class;

        bool TryGet<TValue>(object item, string path, [MaybeNullWhen(false)] out TValue value);

        bool Contains(object item, string path);

        TValue AddOrUpdate<TItem, TValue, TState>(TItem item, string path, TValue addValue, TState state, UpdateValueDelegate<TItem, TValue, TValue, TState, TValue> updateValueFactory)
            where TItem : class;

        TValue AddOrUpdate<TItem, TValue, TState>(TItem item, string path, TState state, Func<TItem, TState, TValue> addValueFactory, UpdateValueDelegate<TItem, TValue, TState, TValue> updateValueFactory)
            where TItem : class;

        TValue GetOrAdd<TValue>(object item, string path, TValue value);

        TValue GetOrAdd<TItem, TValue, TState>(TItem item, string path, TState state, Func<TItem, TState, TValue> valueFactory)
            where TItem : class;

        void Set<TValue>(object item, string path, TValue value);

        bool Clear(object item, string? path = null);
    }
}