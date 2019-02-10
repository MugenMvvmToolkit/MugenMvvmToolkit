using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Navigation.Presenters.Results;

namespace MugenMvvm.Interfaces.Navigation.Presenters
{
    public interface IChildViewModelPresenter
    {
        int Priority { get; }

        IChildViewModelPresenterResult? TryShow(IViewModelPresenter parentPresenter, IReadOnlyMetadataContext metadata);

        IChildViewModelPresenterResult? TryClose(IViewModelPresenter parentPresenter, IReadOnlyMetadataContext metadata);
    }
}