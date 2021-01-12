using System.Collections.Generic;
using MugenMvvm.Collections;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Metadata
{
    public sealed class EmptyMetadataContext : IReadOnlyMetadataContext
    {
        #region Fields

        public static IReadOnlyMetadataContext Instance = new EmptyMetadataContext();

        #endregion

        #region Constructors

        private EmptyMetadataContext()
        {
        }

        #endregion

        #region Properties

        public int Count => 0;

        #endregion

        #region Implementation of interfaces

        public ItemOrIEnumerable<KeyValuePair<IMetadataContextKey, object?>> GetValues() => default;

        public bool Contains(IMetadataContextKey contextKey) => false;

        public bool TryGetRaw(IMetadataContextKey contextKey, out object? value)
        {
            value = null;
            return false;
        }

        #endregion
    }
}