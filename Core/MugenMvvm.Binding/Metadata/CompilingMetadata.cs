using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Metadata;

namespace MugenMvvm.Binding.Metadata
{
    public static class CompilingMetadata
    {
        #region Fields

        private static IMetadataContextKey<List<string>>? _compilingErrors;

        #endregion

        #region Properties

        [AllowNull]
        public static IMetadataContextKey<List<string>> CompilingErrors
        {
            get => _compilingErrors ??= GetBuilder<List<string>>(nameof(CompilingErrors)).Build();
            set => _compilingErrors = value;
        }

        #endregion

        #region Methods

        private static MetadataContextKey.Builder<T> GetBuilder<T>(string name)
        {
            return MetadataContextKey.Create<T>(typeof(CompilingMetadata), name);
        }

        #endregion
    }
}