using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Metadata;

namespace MugenMvvm.Binding.Metadata
{
    public static class ParsingMetadata
    {
        #region Fields

        private static IMetadataContextKey<List<string>, List<string>>? _parsingErrors;

        #endregion

        #region Properties

        [AllowNull]
        public static IMetadataContextKey<List<string>, List<string>> ParsingErrors
        {
            get => _parsingErrors ??= GetBuilder(_parsingErrors, nameof(ParsingErrors)).Build();
            set => _parsingErrors = value;
        }

        #endregion

        #region Methods

        private static MetadataContextKey.Builder<TGet, TSet> GetBuilder<TGet, TSet>(IMetadataContextKey<TGet, TSet>? _, string name)
        {
            return MetadataContextKey.Create<TGet, TSet>(typeof(ParsingMetadata), name);
        }

        #endregion
    }
}