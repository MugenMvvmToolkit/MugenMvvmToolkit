using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Metadata;

namespace MugenMvvm.Interfaces.Metadata
{
    public interface IReadOnlyMetadataContext : IReadOnlyCollection<MetadataContextValue>
    {
        bool TryGet<T>(IReadOnlyMetadataContextKey<T> contextKey, [MaybeNullWhen(false), NotNullIfNotNull("defaultValue")] out T value, [AllowNull] T defaultValue);

        bool Contains(IMetadataContextKey contextKey);
    }
}