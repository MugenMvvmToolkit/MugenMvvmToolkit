using System.Collections.Generic;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Interfaces.Navigation.Presenters
{
    public interface IViewModelPresenter : IHasListeners<IViewModelPresenterListener>//todo update listener add mediator, remove collection from presenter/manager,
                                                                                     //todo tracer remove to listeners, return empty metadata
    {
        IViewModelPresenterCallbackManager CallbackManager { get; }

        void AddPresenter(IChildViewModelPresenter presenter);

        void RemovePresenter(IChildViewModelPresenter presenter);

        IReadOnlyList<IChildViewModelPresenter> GetPresenters();

        IViewModelPresenterResult Show(IReadOnlyMetadataContext metadata);

        IReadOnlyList<IClosingViewModelPresenterResult> TryClose(IReadOnlyMetadataContext metadata);

        IRestorationViewModelPresenterResult TryRestore(IReadOnlyMetadataContext metadata);
    }
}