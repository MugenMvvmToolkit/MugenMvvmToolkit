using MugenMvvm.Interfaces;

namespace MugenMvvm.Models
{
    public struct ContextValue
    {
        #region Constructors

        public ContextValue(IContextKey key, object? value)
        {
            Should.NotBeNull(key, nameof(key));
            key.Validate(value);
            Key = key;
            Value = value;
        }

        #endregion

        #region Properties

        public IContextKey Key { get; }

        public object? Value { get; }

        #endregion
    }
}