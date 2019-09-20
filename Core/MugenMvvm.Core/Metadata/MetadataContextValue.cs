using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Metadata
{
    [StructLayout(LayoutKind.Auto)]
    public readonly struct MetadataContextValue
    {
        #region Fields

        internal static readonly Func<KeyValuePair<IMetadataContextKey, object?>, MetadataContextValue> CreateDelegate = Create;

        public readonly IMetadataContextKey ContextKey;
        public readonly object? Value;

        #endregion

        #region Constructors

        private MetadataContextValue(KeyValuePair<IMetadataContextKey, object?> pair)
            : this(pair.Key, pair.Value)
        {
        }

        private MetadataContextValue(IMetadataContextKey contextKey, object? value)
        {
            ContextKey = contextKey;
            Value = value;
        }

        #endregion

        #region Methods

        public static MetadataContextValue Create(KeyValuePair<IMetadataContextKey, object?> pair)
        {
            Should.NotBeNull(pair.Key, nameof(pair.Key));
            return new MetadataContextValue(pair);
        }

        public static MetadataContextValue Create<T>(IMetadataContextKey<T> contextKey, T value)
        {
            Should.NotBeNull(contextKey, nameof(contextKey));
            return new MetadataContextValue(contextKey, contextKey.SetValue(Default.Metadata, null, value));
        }

        #endregion
    }
}