using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Navigation.Presenters
{
    public interface IConditionViewModelPresenterListener : IViewModelPresenterListener
    {
        bool CanShow(IViewModelPresenter presenter, IChildViewModelPresenter childPresenter, IReadOnlyMetadataContext metadata);

        bool CanClose(IViewModelPresenter presenter, IChildViewModelPresenter childPresenter, IReadOnlyMetadataContext metadata);

        bool CanRestore(IViewModelPresenter presenter, IRestorableChildViewModelPresenter childPresenter, IReadOnlyMetadataContext metadata);
    }
}