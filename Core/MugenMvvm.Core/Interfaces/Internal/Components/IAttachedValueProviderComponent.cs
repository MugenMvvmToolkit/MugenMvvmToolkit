using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Delegates;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Internal.Components
{
    public interface IAttachedValueProviderComponent : IComponent<IAttachedValueProvider>
    {
        bool IsSupported(object item, IReadOnlyMetadataContext? metadata);

        IReadOnlyList<KeyValuePair<string, object?>>? TryGetValues<TItem, TState>(TItem item, TState state, Func<TItem, KeyValuePair<string, object?>, TState, bool>? predicate)
            where TItem : class;

        bool TryGet<TValue>(object item, string path, [NotNullWhen(true)] out TValue value);

        bool Contains(object item, string path);

        [return: NotNullIfNotNull("addValue")]
        TValue AddOrUpdate<TItem, TValue, TState>(TItem item, string path, TValue addValue, TState state, UpdateValueDelegate<TItem, TValue, TValue, TState> updateValueFactory)
            where TItem : class;

        TValue AddOrUpdate<TItem, TValue, TState>(TItem item, string path, TState state, Func<TItem, TState, TValue> addValueFactory,
            UpdateValueDelegate<TItem, Func<TItem, TState, TValue>, TValue, TState> updateValueFactory)
            where TItem : class;

        [return: NotNullIfNotNull("value")]
        TValue GetOrAdd<TValue>(object item, string path, TValue value);

        TValue GetOrAdd<TItem, TValue, TState>(TItem item, string path, TState state, Func<TItem, TState, TValue> valueFactory)
            where TItem : class;

        void Set<TValue>(object item, string path, TValue value);

        bool Clear(object item, string? path);
    }
}