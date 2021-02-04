using System.Threading;
using MugenMvvm.Collections;
using MugenMvvm.Components;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Presentation;
using MugenMvvm.Interfaces.Presentation.Components;

namespace MugenMvvm.Presentation
{
    public sealed class Presenter : ComponentOwnerBase<IPresenter>, IPresenter
    {
        public Presenter(IComponentCollectionManager? componentCollectionManager = null)
            : base(componentCollectionManager)
        {
        }

        public ItemOrIReadOnlyList<IPresenterResult> TryShow(object request, CancellationToken cancellationToken = default, IReadOnlyMetadataContext? metadata = null) =>
            GetComponents<IPresenterComponent>(metadata).TryShow(this, request, cancellationToken, metadata);

        public ItemOrIReadOnlyList<IPresenterResult> TryClose(object request, CancellationToken cancellationToken = default,
            IReadOnlyMetadataContext? metadata = null) =>
            GetComponents<IPresenterComponent>(metadata).TryClose(this, request, cancellationToken, metadata);
    }
}