using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Collections.Internal;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Metadata
{
    public sealed class ReadOnlyMetadataContext : IReadOnlyMetadataContext
    {
        #region Fields

        private readonly MetadataContextLightDictionary _dictionary;

        #endregion

        #region Constructors

        public ReadOnlyMetadataContext(IReadOnlyCollection<MetadataContextValue> values)
        {
            Should.NotBeNull(values, nameof(values));
            _dictionary = new MetadataContextLightDictionary(values.Count);
            foreach (var contextValue in values)
                _dictionary[contextValue.ContextKey] = contextValue.Value;
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

        public IEnumerator<MetadataContextValue> GetEnumerator()
        {
            return _dictionary.Select(MetadataContextValue.CreateDelegate).GetEnumerator();
        }

        public bool TryGet<T>(IMetadataContextKey<T> contextKey, out T value, T defaultValue = default)
        {
            if (_dictionary.TryGetValue(contextKey, out var objValue))
            {
                value = contextKey.GetValue(this, objValue);
                return true;
            }

            value = contextKey.GetDefaultValue(this, defaultValue);
            return false;
        }

        public bool Contains(IMetadataContextKey contextKey)
        {
            return _dictionary.ContainsKey(contextKey);
        }

        #endregion
    }
}