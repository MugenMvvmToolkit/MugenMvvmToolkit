using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.ViewModels;

namespace MugenMvvm.Interfaces.Views
{
    public interface IViewManagerListener
    {
        void OnViewCreated(object view, IViewMappingInfo viewMappingInfo, IReadOnlyMetadataContext metadataContext);

        void OnViewInitialized(object view, IViewModel viewModel, IReadOnlyMetadataContext metadataContext);

        void OnViewCleared(object view, IViewModel viewModel, IReadOnlyMetadataContext metadataContext);
    }
}