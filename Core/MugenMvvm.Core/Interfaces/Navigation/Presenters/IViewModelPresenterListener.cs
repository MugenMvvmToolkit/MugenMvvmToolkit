using System.Collections.Generic;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Navigation.Presenters
{
    public interface IViewModelPresenterListener
    {
        void OnShown(IViewModelPresenter presenter, IReadOnlyMetadataContext metadata, IViewModelPresenterResult result);

        void OnClosed(IViewModelPresenter presenter, IReadOnlyMetadataContext metadata, IReadOnlyList<IClosingViewModelPresenterResult> results);

        void OnRestored(IViewModelPresenter presenter, IReadOnlyMetadataContext metadata, IRestorationViewModelPresenterResult result);
    }
}