using System.Collections.Generic;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Navigation.Presenters
{
    public interface IChildViewModelPresenter
    {
        int Priority { get; }

        IChildViewModelPresenterResult? TryShow(IViewModelPresenter parentPresenter, IReadOnlyMetadataContext metadata);

        IReadOnlyList<IChildViewModelPresenterResult> TryClose(IViewModelPresenter parentPresenter, IReadOnlyMetadataContext metadata);
    }
}