using System;
using System.Collections.Generic;
using MugenMvvm.Delegates;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Metadata;

namespace MugenMvvm.Interfaces.Metadata
{
    public interface IMetadataContext : IReadOnlyMetadataContext, IComponentOwner<IMetadataContext>//todo review
    {
        TGet AddOrUpdate<TGet, TSet, TState>(IMetadataContextKey<TGet, TSet> contextKey, TSet addValue, TState state, UpdateValueDelegate<IMetadataContext, TSet, TGet, TState, TSet> updateValueFactory);

        TGet AddOrUpdate<TGet, TSet, TState>(IMetadataContextKey<TGet, TSet> contextKey, TState state, Func<IMetadataContext, TState, TSet> valueFactory,
            UpdateValueDelegate<IMetadataContext, TGet, TState, TSet> updateValueFactory);

        TGet GetOrAdd<TGet, TSet>(IMetadataContextKey<TGet, TSet> contextKey, TSet value);

        TGet GetOrAdd<TGet, TSet, TState>(IMetadataContextKey<TGet, TSet> contextKey, TState state, Func<IMetadataContext, TState, TSet> valueFactory);

        void Set<TGet, TSet>(IMetadataContextKey<TGet, TSet> contextKey, TSet value, out object? oldValue);

        void Merge(IEnumerable<MetadataContextValue> items);

        bool Clear(IMetadataContextKey contextKey, out object? oldValue);

        void Clear();
    }
}