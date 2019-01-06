using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Presenters.Results;

namespace MugenMvvm.Interfaces.Presenters
{
    public interface IChildViewModelPresenter
    {
        int Priority { get; }

        IChildViewModelPresenterResult? TryShow(IReadOnlyMetadataContext metadata, IViewModelPresenter parentPresenter);

        IClosingViewModelPresenterResult? TryClose(IReadOnlyMetadataContext metadata, IViewModelPresenter parentPresenter);
    }
}