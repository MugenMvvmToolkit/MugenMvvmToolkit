using System;
using System.Collections.Generic;
using MugenMvvm.Collections;
using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Interfaces.Metadata
{
    public interface IMetadataContext : IReadOnlyMetadataContext, IComponentOwner<IMetadataContext>
    {
        T AddOrUpdate<T, TState>(IMetadataContextKey<T> contextKey, T addValue, TState state,
            Func<IMetadataContext, IMetadataContextKey<T>, object?, TState, T> updateValueFactory);

        T AddOrUpdate<T, TState>(IMetadataContextKey<T> contextKey, TState state, Func<IMetadataContext, IMetadataContextKey<T>, TState, T> valueFactory,
            Func<IMetadataContext, IMetadataContextKey<T>, object?, TState, T> updateValueFactory);

        T GetOrAdd<T>(IMetadataContextKey<T> contextKey, T value);

        T GetOrAdd<T, TState>(IMetadataContextKey<T> contextKey, TState state, Func<IMetadataContext, IMetadataContextKey<T>, TState, T> valueFactory);

        bool Set<T>(IMetadataContextKey<T> contextKey, T value, out object? oldValue);

        void Merge(ItemOrIEnumerable<KeyValuePair<IMetadataContextKey, object?>> values);

        bool Remove(IMetadataContextKey contextKey, out object? oldValue);

        void Clear();
    }
}