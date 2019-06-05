using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.ViewModels.Infrastructure
{
    public interface IViewModelProviderViewModelDispatcherComponent : IViewModelDispatcherComponent
    {
        IViewModelBase? TryGetViewModel(IViewModelDispatcher viewModelDispatcher, IReadOnlyMetadataContext metadata);
    }
}