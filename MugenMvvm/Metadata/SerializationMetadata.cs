using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Metadata
{
    public static class SerializationMetadata
    {
        #region Fields

        private static IMetadataContextKey<bool, bool>? _noCache;

        #endregion

        #region Properties

        [AllowNull]
        public static IMetadataContextKey<bool, bool> NoCache
        {
            get => _noCache ??= GetBuilder(_noCache, nameof(NoCache)).Serializable().Build();
            set => _noCache = value;
        }

        #endregion

        #region Methods

        private static MetadataContextKey.Builder<TGet, TSet> GetBuilder<TGet, TSet>(IMetadataContextKey<TGet, TSet>? _, string name)
        {
            return MetadataContextKey.Create<TGet, TSet>(typeof(SerializationMetadata), name);
        }

        #endregion
    }
}