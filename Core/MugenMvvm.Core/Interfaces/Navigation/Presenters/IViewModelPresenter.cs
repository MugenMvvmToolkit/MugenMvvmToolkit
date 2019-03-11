using System.Collections.Generic;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Interfaces.Navigation.Presenters
{
    public interface IViewModelPresenter : IHasListeners<IViewModelPresenterListener>
    {
        IViewModelPresenterCallbackManager CallbackManager { get; }

        IComponentCollection<IChildViewModelPresenter> Presenters { get; }

        IViewModelPresenterResult Show(IReadOnlyMetadataContext metadata);

        IReadOnlyList<IClosingViewModelPresenterResult> TryClose(IReadOnlyMetadataContext metadata);

        IRestorationViewModelPresenterResult TryRestore(IReadOnlyMetadataContext metadata);
    }
}