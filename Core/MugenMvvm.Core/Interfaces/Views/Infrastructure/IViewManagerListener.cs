using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.ViewModels;

namespace MugenMvvm.Interfaces.Views.Infrastructure
{
    public interface IViewManagerListener
    {
        void OnViewCreated(IViewManager viewManager, object view, IViewMappingInfo viewMappingInfo, IReadOnlyMetadataContext metadata);

        void OnViewInitialized(IViewManager viewManager, object view, IViewModel viewModel, IReadOnlyMetadataContext metadata);

        void OnViewCleared(IViewManager viewManager, object view, IViewModel viewModel, IReadOnlyMetadataContext metadata);
    }
}