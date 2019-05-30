using System.Collections.Generic;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Navigation.Presenters
{
    public interface IViewModelPresenterConditionListener : IViewModelPresenterListener
    {
        bool CanShow(IViewModelPresenter presenter, IChildViewModelPresenter childPresenter, IReadOnlyMetadataContext metadata);

        bool CanClose(IViewModelPresenter presenter, IChildViewModelPresenter childPresenter, IReadOnlyList<IChildViewModelPresenterResult> currentResults,
            IReadOnlyMetadataContext metadata);

        bool CanRestore(IViewModelPresenter presenter, IRestorableChildViewModelPresenter childPresenter, IReadOnlyMetadataContext metadata);
    }
}