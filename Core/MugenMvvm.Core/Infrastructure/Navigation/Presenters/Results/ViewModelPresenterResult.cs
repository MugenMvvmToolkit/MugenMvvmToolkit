using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Navigation.Presenters.Results;
using MugenMvvm.Interfaces.ViewModels;

namespace MugenMvvm.Infrastructure.Navigation.Presenters.Results
{
    public class ViewModelPresenterResult : IViewModelPresenterResult
    {
        #region Constructors

        public ViewModelPresenterResult(IViewModelBase viewModel, INavigationCallback showingCallback, INavigationCallback closeCallback, IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(viewModel, nameof(viewModel));
            Should.NotBeNull(showingCallback, nameof(showingCallback));
            Should.NotBeNull(closeCallback, nameof(closeCallback));
            Should.NotBeNull(metadata, nameof(metadata));
            ViewModel = viewModel;
            Metadata = metadata;
            ShowingCallback = showingCallback;
            CloseCallback = closeCallback;
        }

        #endregion

        #region Properties

        public IViewModelBase ViewModel { get; }

        public IReadOnlyMetadataContext Metadata { get; }

        public INavigationCallback ShowingCallback { get; }

        public INavigationCallback CloseCallback { get; }

        #endregion
    }
}