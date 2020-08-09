using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Metadata
{
    public static class ApplicationMetadata
    {
        #region Fields

        private static IMetadataContextKey<bool, bool>? _isInBackground;
        private static IMetadataContextKey<string, string>? _version;

        #endregion

        #region Properties

        [AllowNull]
        public static IMetadataContextKey<bool, bool> IsInBackground
        {
            get => _isInBackground ??= GetBuilder(_isInBackground, nameof(IsInBackground)).Build();
            set => _isInBackground = value;
        }

        [AllowNull]
        public static IMetadataContextKey<string, string> Version
        {
            get => _version ??= GetBuilder(_version, nameof(Version)).DefaultValue("0.0").Build();
            set => _version = value;
        }

        #endregion

        #region Methods

        private static MetadataContextKey.Builder<TGet, TSet> GetBuilder<TGet, TSet>(IMetadataContextKey<TGet, TSet>? _, string name) => MetadataContextKey.Create<TGet, TSet>(typeof(ApplicationMetadata), name);

        #endregion
    }
}