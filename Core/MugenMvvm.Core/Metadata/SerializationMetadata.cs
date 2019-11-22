using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Metadata
{
    public static class SerializationMetadata
    {
        #region Fields

        private static IMetadataContextKey<bool>? _noCache;

        #endregion

        #region Properties

        [AllowNull]
        public static IMetadataContextKey<bool> NoCache
        {
            get => _noCache ??= GetBuilder<bool>(nameof(NoCache)).Serializable().Build();
            set => _noCache = value;
        }

        #endregion

        #region Methods

        private static MetadataContextKey.Builder<T> GetBuilder<T>(string name)
        {
            return MetadataContextKey.Create<T>(typeof(SerializationMetadata), name);
        }

        #endregion
    }
}