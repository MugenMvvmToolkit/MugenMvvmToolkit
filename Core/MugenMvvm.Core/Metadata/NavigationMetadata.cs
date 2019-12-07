using System;
using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.ViewModels;

namespace MugenMvvm.Metadata
{
    public static class NavigationMetadata
    {
        #region Fields

        private static IMetadataContextKey<IViewModelBase>? _viewModel;
        private static IMetadataContextKey<string>? _viewName;
        private static IMetadataContextKey<bool>? _isModal;
        private static IMetadataContextKey<DateTime>? _navigationDate;

        #endregion

        #region Properties

        [AllowNull]
        public static IMetadataContextKey<IViewModelBase> ViewModel
        {
            get => _viewModel ??= GetBuilder<IViewModelBase>(nameof(ViewModel)).NotNull().Serializable().Build();
            set => _viewModel = value;
        }

        [AllowNull]
        public static IMetadataContextKey<string> ViewName
        {
            get => _viewName ??= GetBuilder<string>(nameof(ViewName)).Serializable().NotNull().Build();
            set => _viewName = value;
        }

        [AllowNull]
        public static IMetadataContextKey<bool> IsModal
        {
            get => _isModal ??= GetBuilder<bool>(nameof(IsModal)).Serializable().Build();
            set => _isModal = value;
        }


        [AllowNull]
        public static IMetadataContextKey<DateTime> NavigationDate
        {
            get => _navigationDate ??= GetBuilder<DateTime>(nameof(NavigationDate)).Build();
            set => _navigationDate = value;
        }

        #endregion

        #region Methods

        private static MetadataContextKey.Builder<T> GetBuilder<T>(string name)
        {
            return MetadataContextKey.Create<T>(typeof(NavigationMetadata), name);
        }

        #endregion
    }
}