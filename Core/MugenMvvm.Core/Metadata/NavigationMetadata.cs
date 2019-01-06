using MugenMvvm.Infrastructure.Metadata;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.ViewModels;

// ReSharper disable once CheckNamespace
namespace MugenMvvm
{
    public static class NavigationMetadata
    {
        #region Fields

        private static IMetadataContextKey<IViewModel?> _viewModel;

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

        #endregion

        #region Methods

        private static MetadataContextKey.Builder<T> GetBuilder<T>(string name)
        {
            return MetadataContextKey.Create<T>(typeof(NavigationMetadata), name);
        }

        #endregion
    }
}