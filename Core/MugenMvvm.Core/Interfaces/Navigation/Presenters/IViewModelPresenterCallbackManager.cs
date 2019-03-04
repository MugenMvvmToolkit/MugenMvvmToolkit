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
        void Initialize(IViewModelPresenter presenter);

        INavigationCallback AddCallback(IViewModelBase viewModel, NavigationCallbackType callbackType, IChildViewModelPresenterResult presenterResult,
            IReadOnlyMetadataContext metadata);

        void OnNavigated(INavigationContext navigationContext);

        void OnNavigationFailed(INavigationContext navigationContext, Exception exception);

        void OnNavigationCanceled(INavigationContext navigationContext);

        void OnNavigatingCanceled(INavigationContext navigationContext);

        IReadOnlyList<INavigationCallback> GetCallbacks(INavigationEntry navigationEntry, NavigationCallbackType? callbackType, IReadOnlyMetadataContext metadata);
    }
}