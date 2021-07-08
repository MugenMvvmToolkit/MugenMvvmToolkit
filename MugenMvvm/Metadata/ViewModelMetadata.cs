using System;
using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.ViewModels;

namespace MugenMvvm.Metadata
{
    public static class ViewModelMetadata
    {
        private static IMetadataContextKey<string>? _id;
        private static IMetadataContextKey<IViewModelBase?>? _viewModel;
        private static IMetadataContextKey<IViewModelBase?>? _parentViewModel;

        [AllowNull]
        public static IMetadataContextKey<string> Id
        {
            get => _id ??= GetBuilder(_id, nameof(Id)).DefaultValue(GetId).Serializable().Build();
            set => _id = value;
        }

        [AllowNull]
        public static IMetadataContextKey<IViewModelBase?> ViewModel
        {
            get => _viewModel ??= GetBuilder(_viewModel, nameof(ViewModel)).Serializable().Build();
            set => _viewModel = value;
        }

        [AllowNull]
        public static IMetadataContextKey<IViewModelBase?> ParentViewModel
        {
            get => _parentViewModel ??= GetBuilder(_parentViewModel, nameof(ParentViewModel))
                                        .Getter((context, key, v) => (IViewModelBase?)((IWeakReference?)v)?.Target)
                                        .Setter((context, key, oldValue, v) => v.ToWeakReference())
                                        .Serializable()
                                        .Build();
            set => _parentViewModel = value;
        }

        private static string GetId(IReadOnlyMetadataContext ctx, IMetadataContextKey<string> key, string? value)
        {
            if (value == null && ctx is IMetadataContext context)
                return context.GetOrAdd(key, key, (_, __, ___) => Guid.NewGuid().ToString("n"));
            return value ?? string.Empty;
        }

        private static MetadataContextKey.Builder<T> GetBuilder<T>(IMetadataContextKey<T>? _, string name) => MetadataContextKey.Create<T>(typeof(ViewModelMetadata), name);
    }
}