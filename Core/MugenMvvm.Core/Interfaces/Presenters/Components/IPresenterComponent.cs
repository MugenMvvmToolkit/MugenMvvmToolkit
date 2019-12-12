using System.Collections.Generic;
using System.Threading;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Presenters;

namespace MugenMvvm.Interfaces.Presenters.Components
{
    public interface IPresenterComponent : IComponent<IPresenter>
    {
        PresenterResult TryShow(IReadOnlyMetadataContext metadata, CancellationToken cancellationToken);

        IReadOnlyList<PresenterResult>? TryClose(IReadOnlyMetadataContext metadata, CancellationToken cancellationToken);

        IReadOnlyList<PresenterResult>? TryRestore(IReadOnlyMetadataContext metadata, CancellationToken cancellationToken);
    }
}