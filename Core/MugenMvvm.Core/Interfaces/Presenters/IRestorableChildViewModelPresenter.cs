using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Presenters.Results;

namespace MugenMvvm.Interfaces.Presenters
{
    public interface IRestorableChildViewModelPresenter : IChildViewModelPresenter
    {
        IRestorationViewModelPresenterResult? TryRestore(IReadOnlyMetadataContext metadata, IViewModelPresenter parentPresenter);
    }
}