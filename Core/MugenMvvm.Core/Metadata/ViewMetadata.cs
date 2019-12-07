using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Metadata
{
    public static class ViewMetadata
    {
        #region Fields

        private static IMetadataContextKey<IComponentCollection>? _wrappers;

        #endregion

        #region Properties

        [AllowNull]
        public static IMetadataContextKey<IComponentCollection> Wrappers
        {
            get => _wrappers ??= GetBuilder<IComponentCollection>(nameof(Wrappers)).NotNull().Build();
            set => _wrappers = value;
        }

        #endregion

        #region Methods

        private static MetadataContextKey.Builder<T> GetBuilder<T>(string name)
        {
            return MetadataContextKey.Create<T>(typeof(ViewMetadata), name);
        }

        #endregion
    }
}