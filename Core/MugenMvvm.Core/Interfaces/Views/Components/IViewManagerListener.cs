using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.ViewModels;

namespace MugenMvvm.Interfaces.Views.Components
{
    public interface IViewManagerListener : IComponent<IViewManager>
    {
        void OnViewModelCreated(IViewManager viewManager, IViewModelBase viewModel, object view, IMetadataContext metadata);

        void OnViewCreated(IViewManager viewManager, object view, IViewModelBase viewModel, IMetadataContext metadata);

        void OnViewInitialized(IViewManager viewManager, IViewInfo viewInfo, IViewModelBase viewModel, IMetadataContext metadata);

        void OnViewCleared(IViewManager viewManager, IViewInfo viewInfo, IViewModelBase viewModel, IMetadataContext metadata);
    }
}