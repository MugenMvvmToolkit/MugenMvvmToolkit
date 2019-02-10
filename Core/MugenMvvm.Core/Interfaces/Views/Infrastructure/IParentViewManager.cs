using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.ViewModels;

namespace MugenMvvm.Interfaces.Views.Infrastructure
{
    public interface IParentViewManager : IViewManager
    {
        void OnViewInitialized(IViewModelBase viewModel, object view, IReadOnlyMetadataContext metadata);

        void OnViewCleared(IViewModelBase viewModel, object view, IReadOnlyMetadataContext metadata);
    }
}