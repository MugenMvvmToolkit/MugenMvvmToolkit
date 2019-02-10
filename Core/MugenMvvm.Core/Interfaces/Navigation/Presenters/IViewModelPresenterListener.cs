using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Navigation.Presenters.Results;

namespace MugenMvvm.Interfaces.Navigation.Presenters
{
    public interface IViewModelPresenterListener
    {
        void OnShown(IViewModelPresenter presenter, IReadOnlyMetadataContext metadata, IViewModelPresenterResult result);

        void OnClosed(IViewModelPresenter presenter, IReadOnlyMetadataContext metadata, IClosingViewModelPresenterResult result);

        void OnRestored(IViewModelPresenter presenter, IReadOnlyMetadataContext metadata, IRestorationViewModelPresenterResult result);
    }
}