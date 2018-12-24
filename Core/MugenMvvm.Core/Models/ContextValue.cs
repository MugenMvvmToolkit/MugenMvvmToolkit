using MugenMvvm.Interfaces;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Models
{
    public struct ContextValue
    {
        #region Constructors

        public ContextValue(IMetadataContextKey contextKey, object? value)
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