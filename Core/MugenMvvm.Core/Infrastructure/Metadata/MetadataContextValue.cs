using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Infrastructure.Metadata
{
    public struct MetadataContextValue
    {
        #region Constructors

        public MetadataContextValue(IMetadataContextKey contextKey, object? value)
        {
            Should.NotBeNull(contextKey, nameof(contextKey));
            contextKey.Validate(value);
            ContextKey = contextKey;
            Value = value;
        }

        #endregion

        #region Properties

        public IMetadataContextKey ContextKey { get; }

        public object? Value { get; }

        #endregion
    }
}