using System;
using System.Collections.Generic;
using MugenMvvm.Infrastructure.Metadata;

namespace MugenMvvm.Interfaces.Metadata
{
    public interface IMetadataContext : IReadOnlyMetadataContext
    {
        T GetOrAdd<T>(IMetadataContextKey<T> contextKey, Func<IMetadataContext, object?, object?, T> valueFactory, object? state1, object? state2);

        void Set<T>(IMetadataContextKey<T> contextKey, T value);

        void Merge(IEnumerable<MetadataContextValue> items);

        bool Remove(IMetadataContextKey contextKey);

        void Clear();
    }
}