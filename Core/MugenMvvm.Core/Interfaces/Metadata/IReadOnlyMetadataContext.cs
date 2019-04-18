using System.Collections.Generic;
using MugenMvvm.Infrastructure.Metadata;

namespace MugenMvvm.Interfaces.Metadata
{
    public interface IReadOnlyMetadataContext : IReadOnlyCollection<MetadataContextValue>
    {
        bool TryGet<T>(IMetadataContextKey<T> contextKey, out T value, T defaultValue = default);

        bool Contains(IMetadataContextKey contextKey);
    }
}