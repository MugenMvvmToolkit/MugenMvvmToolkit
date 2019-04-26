using System.Collections.Generic;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Interfaces.Navigation.Presenters
{
    public interface IViewModelPresenterListener : IListener
    {
        void OnShown(IViewModelPresenter presenter, IViewModelPresenterResult result, IReadOnlyMetadataContext metadata);

        void OnClosed(IViewModelPresenter presenter, IReadOnlyList<IClosingViewModelPresenterResult> results, IReadOnlyMetadataContext metadata);

        void OnRestored(IViewModelPresenter presenter, IRestorationViewModelPresenterResult result, IReadOnlyMetadataContext metadata);
    }
}