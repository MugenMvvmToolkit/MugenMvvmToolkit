using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.ViewModels.Infrastructure
{
    public interface IViewModelProviderComponent : IComponent<IViewModelDispatcher>
    {
        IViewModelBase? TryGetViewModel(IReadOnlyMetadataContext metadata);
    }
}