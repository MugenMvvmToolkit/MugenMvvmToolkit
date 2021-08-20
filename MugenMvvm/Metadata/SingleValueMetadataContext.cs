using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Collections;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Metadata
{
    public sealed class SingleValueMetadataContext : IReadOnlyMetadataContext
    {
        private readonly KeyValuePair<IMetadataContextKey, object?> _value;

        public SingleValueMetadataContext(KeyValuePair<IMetadataContextKey, object?> value)
        {
            Should.NotBeNull(value.Key, nameof(value));
            _value = value;
        }

        public int Count => 1;

        public ItemOrIReadOnlyCollection<KeyValuePair<IMetadataContextKey, object?>> GetValues() => _value;

        public bool Contains(IMetadataContextKey contextKey) => _value.Key.Equals(contextKey);

        public bool TryGetRaw(IMetadataContextKey contextKey, [MaybeNullWhen(false)] out object? value)
        {
            if (Contains(contextKey))
            {
                value = _value.Value;
                return true;
            }

            value = null;
            return false;
        }
    }
}