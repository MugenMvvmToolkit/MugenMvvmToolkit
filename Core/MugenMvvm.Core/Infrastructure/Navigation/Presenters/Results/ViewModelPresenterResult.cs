using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Navigation.Presenters;
using MugenMvvm.Interfaces.ViewModels;

// ReSharper disable once CheckNamespace
namespace MugenMvvm.Infrastructure.Navigation.Presenters
{
    public class ViewModelPresenterResult : ChildViewModelPresenterResult, IViewModelPresenterResult
    {
        #region Constructors

        public ViewModelPresenterResult(IViewModelBase viewModel, INavigationCallback showingCallback, INavigationCallback closeCallback, INavigationProvider navigationProvider,
            NavigationType navigationType, IReadOnlyMetadataContext metadata,
            IChildViewModelPresenter? presenter) : base(navigationProvider, navigationType, metadata, presenter)
        {
            Should.NotBeNull(viewModel, nameof(viewModel));
            Should.NotBeNull(showingCallback, nameof(showingCallback));
            Should.NotBeNull(closeCallback, nameof(closeCallback));
            ViewModel = viewModel;
            ShowingCallback = showingCallback;
            CloseCallback = closeCallback;
        }

        public ViewModelPresenterResult(IViewModelBase viewModel, INavigationCallback showingCallback, INavigationCallback closeCallback,
            IChildViewModelPresenterResult childResult)
            : this(viewModel, showingCallback, closeCallback, childResult.NavigationProvider, childResult.NavigationType, childResult.Metadata, childResult.Presenter)
        {
        }

        #endregion

        #region Properties

        public IViewModelBase ViewModel { get; }

        public INavigationCallback ShowingCallback { get; }

        public INavigationCallback CloseCallback { get; }

        #endregion
    }
}