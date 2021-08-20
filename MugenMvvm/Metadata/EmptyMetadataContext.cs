using System.Collections.Generic;
using MugenMvvm.Collections;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Metadata
{
    public sealed class EmptyMetadataContext : IReadOnlyMetadataContext
    {
        public static IReadOnlyMetadataContext Instance = new EmptyMetadataContext();

        private EmptyMetadataContext()
        {
        }

        public int Count => 0;

        public ItemOrIReadOnlyCollection<KeyValuePair<IMetadataContextKey, object?>> GetValues() => default;

        public bool Contains(IMetadataContextKey contextKey) => false;

        public bool TryGetRaw(IMetadataContextKey contextKey, out object? value)
        {
            value = null;
            return false;
        }
    }
}