using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Navigation.Presenters
{
    public interface IRestorableChildViewModelPresenter : IChildViewModelPresenter
    {
        IChildViewModelPresenterResult? TryRestore(IReadOnlyMetadataContext metadata, IViewModelPresenter parentPresenter);
    }
}