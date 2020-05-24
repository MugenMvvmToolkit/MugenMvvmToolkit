using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.ViewModels;

namespace MugenMvvm.Interfaces.Views.Components
{
    public interface IViewManagerListener : IComponent<IViewManager>//todo lifecycle
    {
        void OnViewInitialized(IViewManager viewManager, IView view, IViewModelBase viewModel, IReadOnlyMetadataContext? metadata);

        void OnViewCleared(IViewManager viewManager, IView view, IViewModelBase viewModel, IReadOnlyMetadataContext? metadata);
    }
}