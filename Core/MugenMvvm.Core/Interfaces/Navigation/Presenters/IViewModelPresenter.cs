using System.Collections.Generic;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Navigation.Presenters.Results;

namespace MugenMvvm.Interfaces.Navigation.Presenters
{
    public interface IViewModelPresenter : IHasListeners<IViewModelPresenterListener>
    {
        ICollection<IChildViewModelPresenter> Presenters { get; }

        IViewModelPresenterCallbackManager CallbackManager { get; }


        IViewModelPresenterResult Show(IReadOnlyMetadataContext metadata);

        IClosingViewModelPresenterResult TryClose(IReadOnlyMetadataContext metadata);

        IRestorationViewModelPresenterResult TryRestore(IReadOnlyMetadataContext metadata);
    }
}