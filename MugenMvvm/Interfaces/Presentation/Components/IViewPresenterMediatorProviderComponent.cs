using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.Views;

namespace MugenMvvm.Interfaces.Presentation.Components
{
    public interface IViewPresenterMediatorProviderComponent : IComponent<IPresenter>
    {
        IViewPresenterMediator? TryGetViewPresenter(IPresenter presenter, IViewModelBase viewModel, IViewMapping mapping, IReadOnlyMetadataContext? metadata);
    }
}