using System;
using System.Collections.Generic;
using MugenMvvm.Delegates;
using MugenMvvm.Infrastructure.Metadata;

namespace MugenMvvm.Interfaces.Metadata
{
    public interface IMetadataContext : IReadOnlyMetadataContext
    {
        T AddOrUpdate<T, TState1, TState2>(IMetadataContextKey<T> contextKey, T addValue, UpdateValueDelegate<IMetadataContext, T, T, TState1, TState2> updateValueFactory,
            TState1 state1, TState2 state2);

        T AddOrUpdate<T, TState1, TState2>(IMetadataContextKey<T> contextKey, Func<IMetadataContext, TState1, TState2, T> valueFactory,
            UpdateValueDelegate<IMetadataContext, Func<IMetadataContext, TState1, TState2, T>, T, TState1, TState2> updateValueFactory, TState1 state1, TState2 state2);

        T GetOrAdd<T>(IMetadataContextKey<T> contextKey, T value);

        T GetOrAdd<T, TState1, TState2>(IMetadataContextKey<T> contextKey, Func<IMetadataContext, TState1, TState2, T> valueFactory, TState1 state1, TState2 state2);

        void Set<T>(IMetadataContextKey<T> contextKey, T value);

        void Merge(IEnumerable<MetadataContextValue> items);

        bool Remove(IMetadataContextKey contextKey);

        void Clear();
    }
}