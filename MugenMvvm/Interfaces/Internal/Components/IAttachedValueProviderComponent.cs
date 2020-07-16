using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Delegates;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Interfaces.Internal.Components
{
    public interface IAttachedValueProviderComponent : IComponent<IAttachedValueManager>//todo first parameter
    {
        bool IsSupported(object item, IReadOnlyMetadataContext? metadata);

        ItemOrList<KeyValuePair<string, object?>, IReadOnlyList<KeyValuePair<string, object?>>> TryGetValues<TItem, TState>(TItem item, in TState state, Func<TItem, KeyValuePair<string, object?>, TState, bool>? predicate)
            where TItem : class;

        bool TryGet(object item, string path, out object? value);

        bool Contains(object item, string path);

        TValue AddOrUpdate<TItem, TValue, TState>(TItem item, string path, TValue addValue, in TState state, UpdateValueDelegate<TItem, TValue, TValue, TState, TValue> updateValueFactory)
            where TItem : class;

        TValue AddOrUpdate<TItem, TValue, TState>(TItem item, string path, in TState state, Func<TItem, TState, TValue> addValueFactory, UpdateValueDelegate<TItem, TValue, TState, TValue> updateValueFactory)
            where TItem : class;

        TValue GetOrAdd<TValue>(object item, string path, TValue value);

        TValue GetOrAdd<TItem, TValue, TState>(TItem item, string path, in TState state, Func<TItem, TState, TValue> valueFactory)
            where TItem : class;

        void Set(object item, string path, object? value, out object? oldValue);

        bool Clear(object item, string path, out object? oldValue);

        bool Clear(object item);
    }
}