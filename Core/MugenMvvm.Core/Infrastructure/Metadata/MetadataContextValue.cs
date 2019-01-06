using System.Collections.Generic;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Infrastructure.Metadata
{
    public struct MetadataContextValue
    {
        #region Constructors

        public MetadataContextValue(KeyValuePair<IMetadataContextKey, object?> pair)
            : this(pair.Key, pair.Value)
        {
        }

        private MetadataContextValue(IMetadataContextKey contextKey, object? value)
        {
            ContextKey = contextKey;
            Value = value;
        }

        #endregion

        #region Properties

        public IMetadataContextKey ContextKey { get; }

        public object? Value { get; }

        #endregion

        #region Methods

        public static MetadataContextValue Create<T>(IMetadataContextKey<T> contextKey, T value)
        {
            Should.NotBeNull(contextKey, nameof(contextKey));
            return new MetadataContextValue(contextKey, contextKey.SetValue(Default.MetadataContext, null, value));
        }

        #endregion
    }
}