using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.ViewModels;

namespace MugenMvvm.Interfaces.Views.Infrastructure
{
    public interface IViewManagerListener : IListener
    {
        void OnViewModelCreated(IViewManager viewManager, IViewModelBase viewModel, object view, IReadOnlyMetadataContext metadata);

        void OnViewCreated(IViewManager viewManager, object view, IViewModelBase viewModel, IReadOnlyMetadataContext metadata);//todo view to IViewInfo?

        void OnViewInitialized(IViewManager viewManager, IViewModelBase viewModel, IViewInfo viewInfo, IReadOnlyMetadataContext metadata);

        void OnViewCleared(IViewManager viewManager, IViewModelBase viewModel, IViewInfo viewInfo, IReadOnlyMetadataContext metadata);
    }
}