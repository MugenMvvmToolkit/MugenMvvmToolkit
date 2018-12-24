using System.Collections.Generic;
using MugenMvvm.Models;

namespace MugenMvvm.Interfaces.Metadata
{
    public interface IMetadataContext : IReadOnlyMetadataContext
    {
        void Set(IMetadataContextKey contextKey, object? value);

        void Set<T>(IMetadataContextKey<T> contextKey, T value);

        void Merge(IEnumerable<ContextValue> items);

        bool Remove(IMetadataContextKey contextKey);

        void Clear();
    }
}