using MugenMvvm.Infrastructure.Metadata;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.ViewModels;

namespace MugenMvvm.Metadata
{
    public static class NavigationMetadata
    {
        #region Fields

        private static IMetadataContextKey<IViewModelBase?> _viewModel;
        private static IMetadataContextKey<string?> _viewName;        
        private static IMetadataContextKey<bool> _isDialog;

        #endregion

        #region Properties

        public static IMetadataContextKey<IViewModelBase?> ViewModel
        {
            get
            {
                if (_viewModel == null)
                    _viewModel = GetBuilder<IViewModelBase?>(nameof(ViewModel)).NotNull().Serializable().Build();
                return _viewModel;
            }
            set => _viewModel = value;
        }

        public static IMetadataContextKey<string?> ViewName
        {
            get
            {
                if (_viewName == null)
                    _viewName = GetBuilder<string?>(nameof(ViewName)).Serializable().Build();
                return _viewName;
            }
            set => _viewName = value;
        }

        public static IMetadataContextKey<bool> IsDialog
        {
            get
            {
                if (_isDialog == null)
                    _isDialog = GetBuilder<bool>(nameof(IsDialog)).Serializable().Build();
                return _isDialog;
            }
            set => _isDialog = value;
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