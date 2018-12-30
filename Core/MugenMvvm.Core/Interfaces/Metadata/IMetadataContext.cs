using System.Collections.Generic;
using MugenMvvm.Infrastructure.Metadata;

namespace MugenMvvm.Interfaces.Metadata
{
    public interface IMetadataContext : IReadOnlyMetadataContext
    {
        void Set<T>(IMetadataContextKey<T> contextKey, T value);

        void Merge(IEnumerable<MetadataContextValue> items);

        bool Remove(IMetadataContextKey contextKey);

        void Clear();
    }
}