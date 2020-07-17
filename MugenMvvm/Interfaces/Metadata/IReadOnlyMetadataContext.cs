using System.Collections.Generic;
using MugenMvvm.Metadata;

namespace MugenMvvm.Interfaces.Metadata
{
    public interface IReadOnlyMetadataContext : IReadOnlyCollection<MetadataContextValue>
    {
        bool TryGetRaw(IMetadataContextKey contextKey, out object? value);

        bool Contains(IMetadataContextKey contextKey);
    }
}