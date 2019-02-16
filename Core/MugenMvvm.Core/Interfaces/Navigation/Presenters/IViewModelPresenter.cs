using System.Collections.Generic;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Interfaces.Navigation.Presenters
{
    public interface IViewModelPresenter : IHasListeners<IViewModelPresenterListener>
    {
        ICollection<IChildViewModelPresenter> Presenters { get; }

        IViewModelPresenterCallbackManager CallbackManager { get; }


        IViewModelPresenterResult Show(IReadOnlyMetadataContext metadata);

        IReadOnlyList<IClosingViewModelPresenterResult> TryClose(IReadOnlyMetadataContext metadata);

        IRestorationViewModelPresenterResult TryRestore(IReadOnlyMetadataContext metadata);
    }
}