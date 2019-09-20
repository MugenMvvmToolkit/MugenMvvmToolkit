using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.Views;

namespace MugenMvvm.Interfaces.Presenters.Components
{
    public interface IViewModelMediatorProviderComponent : IComponent<IPresenter>
    {
        IViewModelPresenterMediator? TryGetMediator(IViewModelBase viewModel, IViewInitializer viewInitializer, IReadOnlyMetadataContext? metadata);
    }
}