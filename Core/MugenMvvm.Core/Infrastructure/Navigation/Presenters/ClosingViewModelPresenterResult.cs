using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Navigation.Presenters;

namespace MugenMvvm.Infrastructure.Navigation.Presenters
{
    public class ClosingViewModelPresenterResult : ChildViewModelPresenterResult, IClosingViewModelPresenterResult
    {
        #region Constructors

        public ClosingViewModelPresenterResult(INavigationCallback<bool> closingCallback, INavigationProvider navigationProvider, NavigationType navigationType,
            IReadOnlyMetadataContext metadata, IChildViewModelPresenter? presenter)
            : base(navigationProvider, navigationType, metadata, presenter)
        {
            Should.NotBeNull(closingCallback, nameof(closingCallback));
            ClosingCallback = closingCallback;
        }

        public ClosingViewModelPresenterResult(INavigationCallback<bool> closingCallback, IChildViewModelPresenterResult childResult)
            : this(closingCallback, childResult.NavigationProvider, childResult.NavigationType, childResult.Metadata, childResult.Presenter)
        {
        }

        #endregion

        #region Properties

        public INavigationCallback<bool> ClosingCallback { get; }

        #endregion
    }
}