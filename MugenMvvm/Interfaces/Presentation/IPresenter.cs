using System.Threading;
using MugenMvvm.Collections;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Presentation
{
    public interface IPresenter : IComponentOwner<IPresenter>
    {
        ItemOrIReadOnlyList<IPresenterResult> TryShow(object request, CancellationToken cancellationToken = default, IReadOnlyMetadataContext? metadata = null);

        ItemOrIReadOnlyList<IPresenterResult> TryClose(object request, CancellationToken cancellationToken = default, IReadOnlyMetadataContext? metadata = null);
    }
}