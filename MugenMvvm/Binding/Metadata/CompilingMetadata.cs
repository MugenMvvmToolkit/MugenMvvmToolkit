using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Metadata;

namespace MugenMvvm.Binding.Metadata
{
    public static class CompilingMetadata
    {
        #region Fields

        private static IMetadataContextKey<List<string>, List<string>>? _compilingErrors;
        private static IMetadataContextKey<IParameterInfo, IParameterInfo>? _lambdaParameter;

        #endregion

        #region Properties

        [AllowNull]
        public static IMetadataContextKey<List<string>, List<string>> CompilingErrors
        {
            get => _compilingErrors ??= GetBuilder(_compilingErrors, nameof(CompilingErrors)).Build();
            set => _compilingErrors = value;
        }

        [AllowNull]
        public static IMetadataContextKey<IParameterInfo, IParameterInfo> LambdaParameter
        {
            get => _lambdaParameter ??= GetBuilder(_lambdaParameter, nameof(LambdaParameter)).Build();
            set => _lambdaParameter = value;
        }

        #endregion

        #region Methods

        private static MetadataContextKey.Builder<TGet, TSet> GetBuilder<TGet, TSet>(IMetadataContextKey<TGet, TSet>? _, string name) => MetadataContextKey.Create<TGet, TSet>(typeof(CompilingMetadata), name);

        #endregion
    }
}