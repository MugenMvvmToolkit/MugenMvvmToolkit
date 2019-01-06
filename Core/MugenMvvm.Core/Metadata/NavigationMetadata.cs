using MugenMvvm.Infrastructure.Metadata;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.ViewModels;

// ReSharper disable once CheckNamespace
namespace MugenMvvm
{
    public static class NavigationMetadata
    {
        #region Fields

        private static IMetadataContextKey<IViewModel?> _viewModel;
        private static IMetadataContextKey<bool> _suppressTabNavigation;
        private static IMetadataContextKey<bool> _suppressWindowNavigation;
        private static IMetadataContextKey<bool> _suppressPageNavigation;
        private static IMetadataContextKey<string?> _viewName;
        private static IMetadataContextKey<INavigationWindowMediator?> _navigationWindowMediator;
        private static IMetadataContextKey<object?> _restoredWindowView;

        #endregion

        #region Properties

        public static IMetadataContextKey<IViewModel?> ViewModel
        {
            get
            {
                if (_viewModel == null)
                    _viewModel = GetBuilder<IViewModel?>(nameof(ViewModel)).NotNull().Serializable().Build();
                return _viewModel;
            }
            set => _viewModel = value;
        }

        public static IMetadataContextKey<bool> SuppressTabNavigation
        {
            get
            {
                if (_suppressTabNavigation == null)
                    _suppressTabNavigation = GetBuilder<bool>(nameof(SuppressTabNavigation)).Serializable().Build();
                return _suppressTabNavigation;
            }
            set => _suppressTabNavigation = value;
        }

        public static IMetadataContextKey<bool> SuppressWindowNavigation
        {
            get
            {
                if (_suppressWindowNavigation == null)
                    _suppressWindowNavigation = GetBuilder<bool>(nameof(SuppressWindowNavigation)).Serializable().Build();
                return _suppressWindowNavigation;
            }
            set => _suppressWindowNavigation = value;
        }

        public static IMetadataContextKey<bool> SuppressPageNavigation
        {
            get
            {
                if (_suppressPageNavigation == null)
                    _suppressPageNavigation = GetBuilder<bool>(nameof(SuppressPageNavigation)).Serializable().Build();
                return _suppressPageNavigation;
            }
            set => _suppressPageNavigation = value;
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

        public static IMetadataContextKey<INavigationWindowMediator?> NavigationWindowMediator
        {
            get
            {
                if (_navigationWindowMediator == null)
                    _navigationWindowMediator = GetBuilder<INavigationWindowMediator?>(nameof(NavigationWindowMediator)).Build();
                return _navigationWindowMediator;
            }
            set => _navigationWindowMediator = value;
        }

        public static IMetadataContextKey<object?> RestoredWindowView
        {
            get
            {
                if (_restoredWindowView == null)
                    _restoredWindowView = GetBuilder<object?>(nameof(RestoredWindowView)).NotNull().Build();
                return _restoredWindowView;
            }
            set => _restoredWindowView = value;
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