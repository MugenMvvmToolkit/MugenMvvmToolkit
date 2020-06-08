using System;
using System.Collections.Generic;
using MugenMvvm.Delegates;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Metadata;

namespace MugenMvvm.Interfaces.Metadata
{
    public interface IMetadataContext : IReadOnlyMetadataContext, IComponentOwner<IMetadataContext>
    {
        TGet AddOrUpdate<TGet, TSet, TState>(IMetadataContextKey<TGet, TSet> contextKey, TSet addValue, in TState state, UpdateValueDelegate<IMetadataContext, TSet, TGet, TState, TSet> updateValueFactory);

        TGet AddOrUpdate<TGet, TSet, TState>(IMetadataContextKey<TGet, TSet> contextKey, in TState state, Func<IMetadataContext, TState, TSet> valueFactory,
            UpdateValueDelegate<IMetadataContext, TGet, TState, TSet> updateValueFactory);

        TGet GetOrAdd<TGet, TSet>(IMetadataContextKey<TGet, TSet> contextKey, TSet value);

        TGet GetOrAdd<TGet, TSet, TState>(IMetadataContextKey<TGet, TSet> contextKey, in TState state, Func<IMetadataContext, TState, TSet> valueFactory);

        void Set<TGet, TSet>(IMetadataContextKey<TGet, TSet> contextKey, TSet value);

        void Merge(IEnumerable<MetadataContextValue> items);

        bool Clear(IMetadataContextKey contextKey);

        void Clear();
    }
}