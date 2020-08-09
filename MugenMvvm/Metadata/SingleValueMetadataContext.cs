using System.Collections;
using System.Collections.Generic;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Metadata
{
    public sealed class SingleValueMetadataContext : IReadOnlyMetadataContext
    {
        #region Fields

        private readonly KeyValuePair<IMetadataContextKey, object?> _value;

        #endregion

        #region Constructors

        public SingleValueMetadataContext(KeyValuePair<IMetadataContextKey, object?> value)
        {
            Should.NotBeNull(value.Key, nameof(value));
            _value = value;
        }

        #endregion

        #region Properties

        public int Count => 1;

        #endregion

        #region Implementation of interfaces

        public IEnumerator<KeyValuePair<IMetadataContextKey, object?>> GetEnumerator() => Default.SingleValueEnumerator(_value);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public bool TryGetRaw(IMetadataContextKey contextKey, out object? value)
        {
            if (Contains(contextKey))
            {
                value = _value.Value;
                return true;
            }

            value = null;
            return false;
        }

        public bool Contains(IMetadataContextKey contextKey) => _value.Key.Equals(contextKey);

        #endregion
    }
}