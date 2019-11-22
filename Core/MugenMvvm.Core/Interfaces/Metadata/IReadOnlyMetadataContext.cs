using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Metadata;

namespace MugenMvvm.Interfaces.Metadata
{
    public interface IReadOnlyMetadataContext : IReadOnlyCollection<MetadataContextValue>
    {
        bool TryGet<T>(IMetadataContextKey<T> contextKey, [NotNullWhen(true), MaybeNull, NotNullIfNotNull("defaultValue")] out T value, T defaultValue = default);

        bool Contains(IMetadataContextKey contextKey);
    }
}