using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Metadata;

namespace MugenMvvm.Bindings.Metadata
{
    public static class ParsingMetadata
    {
        private static IMetadataContextKey<List<string>>? _parsingErrors;

        [AllowNull]
        public static IMetadataContextKey<List<string>> ParsingErrors
        {
            get => _parsingErrors ??= GetBuilder(_parsingErrors, nameof(ParsingErrors)).Build();
            set => _parsingErrors = value;
        }

        private static MetadataContextKey.Builder<T> GetBuilder<T>(IMetadataContextKey<T>? _, string name) => MetadataContextKey.Create<T>(typeof(ParsingMetadata), name);
    }
}