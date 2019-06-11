using System.Collections.Generic;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.Views.Infrastructure;

namespace MugenMvvm.Interfaces.Navigation.Presenters
{
    public interface INavigationMediatorViewModelPresenterManager : IHasPriority
    {
        INavigationMediator? TryGetMediator(INavigationMediatorChildViewModelPresenter presenter, IViewModelBase viewModel, IViewInitializer viewInitializer,
            IReadOnlyMetadataContext metadata);

        IReadOnlyList<IChildViewModelPresenterResult>? TryCloseInternal(INavigationMediatorChildViewModelPresenter presenter, IViewModelBase viewModel,
            IReadOnlyList<INavigationMediator> mediators,
            IReadOnlyMetadataContext metadata);
    }
}