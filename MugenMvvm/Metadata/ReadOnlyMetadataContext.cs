using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Extensions;
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

        public ReadOnlyMetadataContext(ItemOrList<KeyValuePair<IMetadataContextKey, object?>, IEnumerable<KeyValuePair<IMetadataContextKey, object?>>> values)
        {
            Should.NotBeNull(values, nameof(values));
            _dictionary = new Dictionary<IMetadataContextKey, object?>(values.Count, InternalEqualityComparer.MetadataContextKey);
            foreach (var contextValue in values)
                _dictionary[contextValue.Key] = contextValue.Value;
        }

        #endregion

        #region Properties

        public int Count => _dictionary.Count;

        #endregion

        #region Implementation of interfaces

        public ItemOrList<KeyValuePair<IMetadataContextKey, object?>, IEnumerable<KeyValuePair<IMetadataContextKey, object?>>> GetValues() => _dictionary;

        public bool Contains(IMetadataContextKey contextKey) => _dictionary.ContainsKey(contextKey);

        public bool TryGet<T>(IReadOnlyMetadataContextKey<T> contextKey, [MaybeNullWhen(false)] [NotNullIfNotNull("defaultValue")]
            out T value, [AllowNull] T defaultValue)
            => this.TryGetFromRaw(contextKey, _dictionary.TryGetValue(contextKey, out var rawValue), rawValue, out value, defaultValue!);

        #endregion
    }
}