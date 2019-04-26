using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.ViewModels;

namespace MugenMvvm.Interfaces.Navigation.Presenters
{
    public interface IViewModelPresenterCallbackManagerListener : IListener
    {
        void OnCallbackAdded(IViewModelPresenterCallbackManager callbackManager, INavigationCallback callback, IViewModelBase viewModel,
            IChildViewModelPresenterResult presenterResult, IReadOnlyMetadataContext metadata);

        void OnCallbackExecuted(IViewModelPresenterCallbackManager callbackManager, INavigationCallback callback, IViewModelBase viewModel,
            INavigationContext? navigationContext);
    }
}