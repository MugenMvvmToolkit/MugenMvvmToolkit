using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Metadata
{
    public sealed class ReadOnlyMetadataContext : IReadOnlyMetadataContext
    {
        #region Fields

        private readonly Dictionary<IMetadataContextKey, object?> _dictionary;

        #endregion

        #region Constructors

        public ReadOnlyMetadataContext(IReadOnlyCollection<KeyValuePair<IMetadataContextKey, object?>> values)
        {
            Should.NotBeNull(values, nameof(values));
            _dictionary = new Dictionary<IMetadataContextKey, object?>(values.Count, InternalComparer.MetadataContextKey);
            foreach (var contextValue in values)
                _dictionary[contextValue.Key] = contextValue.Value;
        }

        #endregion

        #region Properties

        public int Count => _dictionary.Count;

        #endregion

        #region Implementation of interfaces

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<KeyValuePair<IMetadataContextKey, object?>> GetEnumerator()
        {
            return _dictionary.GetEnumerator();
        }

        public bool TryGetRaw(IMetadataContextKey contextKey, out object? value)
        {
            return _dictionary.TryGetValue(contextKey, out value);
        }

        public bool Contains(IMetadataContextKey contextKey)
        {
            return _dictionary.ContainsKey(contextKey);
        }

        #endregion
    }
}