using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Delegates;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Metadata;

namespace MugenMvvm.Interfaces.Metadata
{
    public interface IMetadataContext : IReadOnlyMetadataContext, IComponentOwner<IMetadataContext>
    {
        [return: NotNullIfNotNull("addValue")]
        T AddOrUpdate<T, TState>(IMetadataContextKey<T> contextKey, T addValue, TState state, UpdateValueDelegate<IMetadataContext, T, T, TState> updateValueFactory);

        T AddOrUpdate<T, TState>(IMetadataContextKey<T> contextKey, TState state, Func<IMetadataContext, TState, T> valueFactory,
            UpdateValueDelegate<IMetadataContext, Func<IMetadataContext, TState, T>, T, TState> updateValueFactory);

        [return: NotNullIfNotNull("value")]
        T GetOrAdd<T>(IMetadataContextKey<T> contextKey, T value);

        T GetOrAdd<T, TState>(IMetadataContextKey<T> contextKey, TState state, Func<IMetadataContext, TState, T> valueFactory);

        void Set<T>(IMetadataContextKey<T> contextKey, T value);

        void Merge(IEnumerable<MetadataContextValue> items);

        bool Clear(IMetadataContextKey? contextKey = null);
    }
}