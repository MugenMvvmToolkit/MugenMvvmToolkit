using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.ViewModels;

namespace MugenMvvm.Interfaces.Navigation.Presenters
{
    public interface IViewModelPresenterCallbackManagerListener : IListener
    {
        void OnCallbackAdded(IViewModelPresenterCallbackManager callbackManager, IViewModelBase viewModel, INavigationCallback callback, IChildViewModelPresenterResult presenterResult);

        void OnCallbackExecuted(IViewModelPresenterCallbackManager callbackManager, IViewModelBase viewModel, INavigationCallback callback, INavigationContext? navigationContext);
    }
}