using System.Collections.Generic;
using System.Threading;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Interfaces.Presenters
{
    public interface IPresenter : IComponentOwner<IPresenter>
    {
        ItemOrList<IPresenterResult, IReadOnlyList<IPresenterResult>> TryShow(object request, CancellationToken cancellationToken = default, IReadOnlyMetadataContext? metadata = null);

        ItemOrList<IPresenterResult, IReadOnlyList<IPresenterResult>> TryClose(object request, CancellationToken cancellationToken = default, IReadOnlyMetadataContext? metadata = null);
    }
}