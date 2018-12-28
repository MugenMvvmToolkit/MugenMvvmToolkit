using System.Collections.Generic;
using MugenMvvm.Infrastructure.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Interfaces.Metadata
{
    public interface IReadOnlyMetadataContext : IReadOnlyCollection<MetadataContextValue>, IHasMemento
    {
        bool TryGet(IMetadataContextKey contextKey, out object? value, object? defaultValue = null);

        bool TryGet<T>(IMetadataContextKey<T> contextKey, out T value, T defaultValue = default);

        bool Contains(IMetadataContextKey contextKey);
    }
}