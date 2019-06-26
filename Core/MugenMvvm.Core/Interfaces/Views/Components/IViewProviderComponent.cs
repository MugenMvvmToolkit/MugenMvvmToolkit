using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.ViewModels;

namespace MugenMvvm.Interfaces.Views
{
    public interface IViewProviderComponent : IComponent<IViewManager>//todo metadataconext?
    {
        object? TryGetViewForViewModel(IViewInitializer initializer, IViewModelBase viewModel, IMetadataContext metadata);
    }
}