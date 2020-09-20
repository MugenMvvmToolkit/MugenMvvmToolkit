using System;
using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.ViewModels;

namespace MugenMvvm.Metadata
{
    public static class ViewModelMetadata
    {
        #region Fields

        private static IMetadataContextKey<string, string>? _id;
        private static IMetadataContextKey<IViewModelBase?, IViewModelBase?>? _viewModel;

        #endregion

        #region Properties

        [AllowNull]
        public static IMetadataContextKey<string, string> Id
        {
            get => _id ??= GetBuilder(_id, nameof(Id)).DefaultValue(GetId).Serializable().Build();
            set => _id = value;
        }

        [AllowNull]
        public static IMetadataContextKey<IViewModelBase?, IViewModelBase?> ViewModel
        {
            get => _viewModel ??= GetBuilder(_viewModel, nameof(ViewModel)).Serializable().Build();
            set => _viewModel = value;
        }

        #endregion

        #region Methods

        private static string GetId(IReadOnlyMetadataContext ctx, IMetadataContextKey<string, string> key, string? value)
        {
            if (value == null && ctx is IMetadataContext context)
                return context.GetOrAdd(key, key, (metadataContext, contextKey) => Guid.NewGuid().ToString("n"));
            return value ?? string.Empty;
        }

        private static MetadataContextKey.Builder<TGet, TSet> GetBuilder<TGet, TSet>(IMetadataContextKey<TGet, TSet>? _, string name) => MetadataContextKey.Create<TGet, TSet>(typeof(ViewModelMetadata), name);

        #endregion
    }
}