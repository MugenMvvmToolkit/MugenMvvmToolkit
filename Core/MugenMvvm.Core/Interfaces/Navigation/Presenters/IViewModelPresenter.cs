using System.Collections.Generic;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Navigation.Presenters.Results;

namespace MugenMvvm.Interfaces.Navigation.Presenters
{
    public interface IViewModelPresenter
    {
        ICollection<IChildViewModelPresenter> Presenters { get; }

        IViewModelPresenterResult Show(IReadOnlyMetadataContext metadata);

        IClosingViewModelPresenterResult TryClose(IReadOnlyMetadataContext metadata);

        IRestorationViewModelPresenterResult TryRestore(IReadOnlyMetadataContext metadata);
    }
}