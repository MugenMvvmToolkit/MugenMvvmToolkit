using System.Collections.Generic;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.ViewModels;

namespace MugenMvvm.Interfaces.Presenters.Components
{
    public interface IViewModelMediatorClosingManagerComponent : IComponent<IPresenter>
    {
        IReadOnlyList<IPresenterResult>? TryCloseInternal(IViewModelBase viewModel, IReadOnlyList<IViewModelPresenterMediator> mediators, IReadOnlyMetadataContext? metadata);
    }
}