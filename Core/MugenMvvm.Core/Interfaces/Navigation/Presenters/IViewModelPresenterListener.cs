using System.Collections.Generic;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Interfaces.Navigation.Presenters
{
    public interface IViewModelPresenterListener : IListener
    {
        void OnChildViewModelPresenterAdded(IViewModelPresenter presenter, IChildViewModelPresenter childPresenter);

        void OnChildViewModelPresenterRemoved(IViewModelPresenter presenter, IChildViewModelPresenter childPresenter);

        void OnShown(IViewModelPresenter presenter, IReadOnlyMetadataContext metadata, IViewModelPresenterResult result);

        void OnClosed(IViewModelPresenter presenter, IReadOnlyMetadataContext metadata, IReadOnlyList<IClosingViewModelPresenterResult> results);

        void OnRestored(IViewModelPresenter presenter, IReadOnlyMetadataContext metadata, IRestorationViewModelPresenterResult result);
    }
}