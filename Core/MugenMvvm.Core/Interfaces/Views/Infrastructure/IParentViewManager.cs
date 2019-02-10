using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.ViewModels;

namespace MugenMvvm.Interfaces.Views.Infrastructure
{
    public interface IParentViewManager : IViewManager
    {
        void OnViewInitialized(IViewModel viewModel, object view, IReadOnlyMetadataContext metadata);

        void OnViewCleared(IViewModel viewModel, object view, IReadOnlyMetadataContext metadata);
    }
}