using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.ViewModels;

namespace MugenMvvm.Interfaces.Views.Infrastructure
{
    public interface IViewManagerListener : IListener
    {
        void OnChildViewManagerAdded(IViewManager viewManager, IChildViewManager childViewManager);

        void OnChildViewManagerRemoved(IViewManager viewManager, IChildViewManager childViewManager);

        void OnViewModelCreated(IViewManager viewManager, IViewModelBase viewModel, object view, IReadOnlyMetadataContext metadata);

        void OnViewCreated(IViewManager viewManager, IViewModelBase viewModel, object view, IReadOnlyMetadataContext metadata);

        void OnViewInitialized(IViewManager viewManager, IViewModelBase viewModel, IViewInfo viewInfo, IReadOnlyMetadataContext metadata);

        void OnViewCleared(IViewManager viewManager, IViewModelBase viewModel, IViewInfo viewInfo, IReadOnlyMetadataContext metadata);
    }
}