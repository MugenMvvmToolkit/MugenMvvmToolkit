using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Navigation.Presenters.Results;

namespace MugenMvvm.Interfaces.Navigation.Presenters
{
    public interface IRestorableChildViewModelPresenter : IChildViewModelPresenter
    {
        IChildViewModelPresenterResult? TryRestore(IViewModelPresenter parentPresenter, IReadOnlyMetadataContext metadata);
    }
}