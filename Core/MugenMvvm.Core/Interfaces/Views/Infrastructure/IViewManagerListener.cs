using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.ViewModels;

namespace MugenMvvm.Interfaces.Views.Infrastructure
{
    public interface IViewManagerListener
    {
        void OnViewInitialized(IViewManager viewManager, IViewModel viewModel, object view, IReadOnlyMetadataContext metadata);

        void OnViewCleared(IViewManager viewManager, IViewModel viewModel, object view, IReadOnlyMetadataContext metadata);
    }
}