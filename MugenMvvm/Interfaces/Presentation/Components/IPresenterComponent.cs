using System.Threading;
using MugenMvvm.Collections;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Presentation.Components
{
    public interface IPresenterComponent : IComponent<IPresenter>
    {
        ItemOrIReadOnlyList<IPresenterResult> TryShow(IPresenter presenter, object request, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata);

        ItemOrIReadOnlyList<IPresenterResult> TryClose(IPresenter presenter, object request, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata);
    }
}