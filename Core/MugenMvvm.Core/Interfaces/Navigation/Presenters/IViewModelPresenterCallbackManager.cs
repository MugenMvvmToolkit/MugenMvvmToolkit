using System;
using System.Collections.Generic;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.ViewModels;

namespace MugenMvvm.Interfaces.Navigation.Presenters
{
    public interface IViewModelPresenterCallbackManager : IHasListeners<IViewModelPresenterCallbackManagerListener>
    {
        INavigationCallback AddCallback(IViewModelPresenter presenter, IViewModelBase viewModel, NavigationCallbackType callbackType,
            IChildViewModelPresenterResult presenterResult, IReadOnlyMetadataContext metadata);

        void OnNavigated(IViewModelPresenter presenter, INavigationContext navigationContext);

        void OnNavigationFailed(IViewModelPresenter presenter, INavigationContext navigationContext, Exception exception);

        void OnNavigationCanceled(IViewModelPresenter presenter, INavigationContext navigationContext);

        void OnNavigatingCanceled(IViewModelPresenter presenter, INavigationContext navigationContext);

        IReadOnlyList<INavigationCallback> GetCallbacks(IViewModelPresenter presenter, INavigationEntry navigationEntry, NavigationCallbackType? callbackType, IReadOnlyMetadataContext metadata);
    }
}