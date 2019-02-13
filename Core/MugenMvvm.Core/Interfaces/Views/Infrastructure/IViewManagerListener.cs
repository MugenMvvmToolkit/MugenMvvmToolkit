using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.ViewModels;

namespace MugenMvvm.Interfaces.Views.Infrastructure
{
    public interface IViewManagerListener
    {
        void OnViewModelCreated(IViewManager viewManager, IViewModelBase viewModel, object view, IReadOnlyMetadataContext metadata);

        void OnViewCreated(IViewManager viewManager, IViewModelBase viewModel, object view, IReadOnlyMetadataContext metadata);

        void OnViewInitialized(IViewManager viewManager, IViewModelBase viewModel, IViewInfo viewInfo, IReadOnlyMetadataContext metadata);

        void OnViewCleared(IViewManager viewManager, IViewModelBase viewModel, IViewInfo viewInfo, IReadOnlyMetadataContext metadata);
    }
}