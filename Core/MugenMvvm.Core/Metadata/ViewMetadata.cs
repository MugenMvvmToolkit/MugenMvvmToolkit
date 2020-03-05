using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Metadata
{
    public static class ViewMetadata
    {
        #region Fields

        private static IMetadataContextKey<IComponentCollection, IComponentCollection>? _wrappers;

        #endregion

        #region Properties

        [AllowNull]
        public static IMetadataContextKey<IComponentCollection, IComponentCollection> Wrappers
        {
            get => _wrappers ??= GetBuilder(_wrappers, nameof(Wrappers)).NotNull().Build();
            set => _wrappers = value;
        }

        #endregion

        #region Methods

        private static MetadataContextKey.Builder<TGet, TSet> GetBuilder<TGet, TSet>(IMetadataContextKey<TGet, TSet>? _, string name)
        {
            return MetadataContextKey.Create<TGet, TSet>(typeof(ViewMetadata), name);
        }

        #endregion
    }
}