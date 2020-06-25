using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Metadata
{
    public static class ViewMetadata
    {
        #region Fields

        private static IMetadataContextKey<ViewLifecycleState, ViewLifecycleState>? _lifecycleState;

        #endregion

        #region Properties

        [AllowNull]
        public static IMetadataContextKey<ViewLifecycleState, ViewLifecycleState> LifecycleState
        {
            get => _lifecycleState ??= GetBuilder(_lifecycleState, nameof(LifecycleState)).NotNull().Build();
            set => _lifecycleState = value;
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