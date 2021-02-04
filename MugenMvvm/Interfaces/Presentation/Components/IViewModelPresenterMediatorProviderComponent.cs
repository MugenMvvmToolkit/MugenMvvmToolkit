using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.Views;

namespace MugenMvvm.Interfaces.Presentation.Components
{
    public interface IViewModelPresenterMediatorProviderComponent : IComponent<IPresenter>
    {
        IViewModelPresenterMediator? TryGetPresenterMediator(IPresenter presenter, IViewModelBase viewModel, IViewMapping mapping, IReadOnlyMetadataContext? metadata);
    }
}