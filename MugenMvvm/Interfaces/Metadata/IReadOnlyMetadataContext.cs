using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace MugenMvvm.Interfaces.Metadata
{
    public interface IReadOnlyMetadataContext : IReadOnlyCollection<KeyValuePair<IMetadataContextKey, object?>>
    {
        bool Contains(IMetadataContextKey contextKey);

        bool TryGet<T>(IReadOnlyMetadataContextKey<T> contextKey, [MaybeNullWhen(false)][NotNullIfNotNull("defaultValue")] out T value, [AllowNull] T defaultValue);
    }
}