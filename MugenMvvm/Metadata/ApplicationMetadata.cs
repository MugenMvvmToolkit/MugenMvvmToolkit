using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Metadata
{
    public static class ApplicationMetadata
    {
        #region Fields

        private static IMetadataContextKey<bool, bool>? _isInBackground;

        #endregion

        #region Properties

        [AllowNull]
        public static IMetadataContextKey<bool, bool> IsInBackground
        {
            get => _isInBackground ??= GetBuilder(_isInBackground, nameof(IsInBackground)).Build();
            set => _isInBackground = value;
        }

        #endregion

        #region Methods

        private static MetadataContextKey.Builder<TGet, TSet> GetBuilder<TGet, TSet>(IMetadataContextKey<TGet, TSet>? _, string name) => MetadataContextKey.Create<TGet, TSet>(typeof(ApplicationMetadata), name);

        #endregion
    }
}