using MugenMvvm.Interfaces.Navigation.Presenters.Results;
using MugenMvvm.Interfaces.ViewModels;

namespace MugenMvvm.Interfaces.Navigation.Presenters
{
    public interface IViewModelPresenterCallbackManagerListener
    {
        void OnCallbackAdded(IViewModelPresenterCallbackManager callbackManager, IViewModel viewModel, INavigationCallback callback, IChildViewModelPresenterResult presenterResult);

        void OnCallbackExecuted(IViewModelPresenterCallbackManager callbackManager, IViewModel viewModel, INavigationCallback callback, INavigationContext? navigationContext);
    }
}