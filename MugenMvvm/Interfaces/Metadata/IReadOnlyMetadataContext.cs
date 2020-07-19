using System.Collections.Generic;
using MugenMvvm.Metadata;

namespace MugenMvvm.Interfaces.Metadata
{
    public interface IReadOnlyMetadataContext : IReadOnlyCollection<MetadataContextValue>
    {
        bool Contains(IMetadataContextKey contextKey);

        bool TryGetRaw(IMetadataContextKey contextKey, out object? value);
    }
}