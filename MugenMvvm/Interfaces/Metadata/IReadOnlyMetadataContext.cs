using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Collections;

namespace MugenMvvm.Interfaces.Metadata
{
    public interface IReadOnlyMetadataContext
    {
        int Count { get; }

        ItemOrIEnumerable<KeyValuePair<IMetadataContextKey, object?>> GetValues();

        bool Contains(IMetadataContextKey contextKey);

        bool TryGetRaw(IMetadataContextKey contextKey, [MaybeNullWhen(false)] out object? value);
    }
}