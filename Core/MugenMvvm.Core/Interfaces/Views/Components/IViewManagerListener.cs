using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.ViewModels;

namespace MugenMvvm.Interfaces.Views
{
    public interface IViewManagerListener : IComponent<IViewManager>
    {
        void OnViewModelCreated(IViewManager viewManager, IViewModelBase viewModel, object view, IReadOnlyMetadataContext metadata);//todo non readonly metadata check?

        void OnViewCreated(IViewManager viewManager, object view, IViewModelBase viewModel, IReadOnlyMetadataContext metadata);

        void OnViewInitialized(IViewManager viewManager, IViewInfo viewInfo, IViewModelBase viewModel, IReadOnlyMetadataContext metadata);

        void OnViewCleared(IViewManager viewManager, IViewInfo viewInfo, IViewModelBase viewModel, IReadOnlyMetadataContext metadata);
    }
}