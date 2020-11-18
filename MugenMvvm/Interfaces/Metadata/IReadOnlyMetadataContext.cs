using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Internal;

namespace MugenMvvm.Interfaces.Metadata
{
    public interface IReadOnlyMetadataContext
    {
        int Count { get; }

        ItemOrList<KeyValuePair<IMetadataContextKey, object?>, IEnumerable<KeyValuePair<IMetadataContextKey, object?>>> GetValues();

        bool Contains(IMetadataContextKey contextKey);

        bool TryGet<T>(IReadOnlyMetadataContextKey<T> contextKey, [MaybeNullWhen(false)] [NotNullIfNotNull("defaultValue")]
            out T value, [AllowNull] T defaultValue);
    }
}