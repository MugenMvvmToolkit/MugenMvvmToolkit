using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.ViewModels;

namespace MugenMvvm.Interfaces.Views.Components
{
    public interface IViewProviderComponent : IComponent<IViewManager>
    {
        object? TryGetViewForViewModel(IViewInitializer initializer, IViewModelBase viewModel, IMetadataContext metadata);
    }
}