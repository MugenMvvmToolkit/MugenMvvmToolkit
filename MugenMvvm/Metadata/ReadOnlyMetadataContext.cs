using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Collections;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Metadata
{
    public sealed class ReadOnlyMetadataContext : IReadOnlyMetadataContext
    {
        private readonly Dictionary<IMetadataContextKey, object?> _dictionary;

        public ReadOnlyMetadataContext(ItemOrIEnumerable<KeyValuePair<IMetadataContextKey, object?>> values)
        {
            _dictionary = new Dictionary<IMetadataContextKey, object?>(values.Count, InternalEqualityComparer.MetadataContextKey);
            foreach (var contextValue in values)
                _dictionary[contextValue.Key] = contextValue.Value;
        }

        public int Count => _dictionary.Count;

        public ItemOrIEnumerable<KeyValuePair<IMetadataContextKey, object?>> GetValues() => ItemOrIEnumerable.FromList(_dictionary);

        public bool Contains(IMetadataContextKey contextKey) => _dictionary.ContainsKey(contextKey);

        public bool TryGetRaw(IMetadataContextKey contextKey, [MaybeNullWhen(false)] out object? value) => _dictionary.TryGetValue(contextKey, out value);
    }
}