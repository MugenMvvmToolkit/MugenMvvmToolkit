using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Presenters.Results;

namespace MugenMvvm.Interfaces.Presenters
{
    public interface IChildViewModelPresenter
    {
        int Priority { get; }

        IChildViewModelPresenterResult? TryShowAsync(IReadOnlyMetadataContext metadata, IViewModelPresenter parentPresenter);

        IClosingViewModelPresenterResult? TryCloseAsync(IReadOnlyMetadataContext metadata, IViewModelPresenter parentPresenter);
    }
}