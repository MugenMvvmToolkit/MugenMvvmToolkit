using System.Collections.Generic;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Interfaces.Navigation.Presenters
{
    public interface IViewModelPresenter : IHasListeners<IViewModelPresenterListener>//todo update listener add mediator, remove collection from presenter/manager
    {
        ICollection<IChildViewModelPresenter> Presenters { get; }

        IViewModelPresenterCallbackManager CallbackManager { get; }


        IViewModelPresenterResult Show(IReadOnlyMetadataContext metadata);

        IReadOnlyList<IClosingViewModelPresenterResult> TryClose(IReadOnlyMetadataContext metadata);

        IRestorationViewModelPresenterResult TryRestore(IReadOnlyMetadataContext metadata);
    }  
}