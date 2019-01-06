using System.Collections.Generic;
using System.Threading.Tasks;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Presenters.Results;
using MugenMvvm.Models;

namespace MugenMvvm.Interfaces.Presenters
{
    public interface IViewModelPresenter
    {
        ICollection<IChildViewModelPresenter> Presenters { get; }

        IViewModelPresenterResult Show(IReadOnlyMetadataContext metadata);

        IClosingViewModelPresenterResult TryClose(IReadOnlyMetadataContext metadata);

        IRestorationViewModelPresenterResult TryRestore(IReadOnlyMetadataContext metadata);

        Task WaitOpenNavigationAsync(NavigationType? type, IReadOnlyMetadataContext metadata);
    }
}