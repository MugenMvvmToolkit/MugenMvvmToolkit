using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Metadata;

namespace MugenMvvm.Binding.Metadata
{
    public static class ParsingMetadata
    {
        #region Fields

        private static IMetadataContextKey<List<string>>? _parsingErrors;

        #endregion

        #region Properties

        [AllowNull]
        public static IMetadataContextKey<List<string>> ParsingErrors
        {
            get => _parsingErrors ??= GetBuilder<List<string>>(nameof(ParsingErrors)).Build();
            set => _parsingErrors = value;
        }

        #endregion

        #region Methods

        private static MetadataContextKey.Builder<T> GetBuilder<T>(string name)
        {
            return MetadataContextKey.Create<T>(typeof(ParsingMetadata), name);
        }

        #endregion
    }
}