using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Navigation.Presenters
{
    public interface IRestorableChildViewModelPresenter : IChildViewModelPresenter
    {
        IChildViewModelPresenterResult? TryRestore(IViewModelPresenter parentPresenter, IReadOnlyMetadataContext metadata);
    }
}