using System.Collections.Generic;
using MugenMvvm.Infrastructure.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Models;

namespace MugenMvvm.Interfaces.Metadata
{
    public interface IReadOnlyMetadataContext : IReadOnlyCollection<MetadataContextValue>, IHasMemento
    {
        bool TryGet(IMetadataContextKey contextKey, out object? value);

        bool TryGet<T>(IMetadataContextKey<T> contextKey, out T value);

        bool Contains(IMetadataContextKey contextKey);
    }
}