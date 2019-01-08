using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Navigation.Presenters
{
    public interface IChildViewModelPresenter
    {
        int Priority { get; }

        IChildViewModelPresenterResult? TryShow(IReadOnlyMetadataContext metadata, IViewModelPresenter parentPresenter);

        IChildViewModelPresenterResult? TryClose(IReadOnlyMetadataContext metadata, IViewModelPresenter parentPresenter);
    }
}