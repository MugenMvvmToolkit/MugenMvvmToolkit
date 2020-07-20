using System.Collections.Generic;

namespace MugenMvvm.Interfaces.Metadata
{
    public interface IReadOnlyMetadataContext : IReadOnlyCollection<KeyValuePair<IMetadataContextKey, object?>>
    {
        bool Contains(IMetadataContextKey contextKey);

        bool TryGetRaw(IMetadataContextKey contextKey, out object? value);
    }
}