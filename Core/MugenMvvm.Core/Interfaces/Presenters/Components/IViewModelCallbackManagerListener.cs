using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.ViewModels;

namespace MugenMvvm.Interfaces.Presenters.Components
{
    public interface IViewModelCallbackManagerListener : IComponent<IPresenter>
    {
        void OnCallbackAdded(INavigationCallback callback, IViewModelBase viewModel, IReadOnlyMetadataContext? metadata);

        void OnCallbackExecuted(INavigationCallback callback, IViewModelBase viewModel, INavigationContext? navigationContext);
    }
}