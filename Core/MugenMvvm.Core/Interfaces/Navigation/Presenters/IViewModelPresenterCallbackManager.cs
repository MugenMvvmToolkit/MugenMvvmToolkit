using System;
using System.Collections.Generic;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Navigation.Presenters.Results;
using MugenMvvm.Interfaces.ViewModels;

namespace MugenMvvm.Interfaces.Navigation.Presenters
{
    public interface IViewModelPresenterCallbackManager : IHasListeners<IViewModelPresenterCallbackManagerListener>
    {
        INavigationCallback AddCallback(IViewModelPresenter presenter, IViewModelBase viewModel, NavigationCallbackType callbackType, IChildViewModelPresenterResult presenterResult);

        void OnNavigated(IViewModelPresenter presenter, INavigationContext context);

        void OnNavigationFailed(IViewModelPresenter presenter, INavigationContext context, Exception exception);

        void OnNavigationCanceled(IViewModelPresenter presenter, INavigationContext context);

        void OnNavigatingCanceled(IViewModelPresenter presenter, INavigationContext context);

        IReadOnlyList<INavigationCallback> GetCallbacks(IViewModelPresenter presenter, INavigationEntry navigationEntry, NavigationCallbackType? callbackType, IReadOnlyMetadataContext metadata);
    }
}