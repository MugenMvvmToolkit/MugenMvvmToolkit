using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using MugenMvvm.Components;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Presenters;
using MugenMvvm.Interfaces.Presenters.Components;
using MugenMvvm.Internal;

namespace MugenMvvm.Presenters
{
    public sealed class Presenter : ComponentOwnerBase<IPresenter>, IPresenter
    {
        #region Constructors

        public Presenter(IComponentCollectionManager? componentCollectionManager = null)
            : base(componentCollectionManager)
        {
        }

        #endregion

        #region Implementation of interfaces

        public ItemOrList<IPresenterResult, IReadOnlyList<IPresenterResult>> TryShow(object request, CancellationToken cancellationToken = default, IReadOnlyMetadataContext? metadata = null)
        {
            return GetComponents<IPresenterComponent>(metadata).TryShow(this, request, cancellationToken, metadata);
        }

        public ItemOrList<IPresenterResult, IReadOnlyList<IPresenterResult>> TryClose(object request, CancellationToken cancellationToken = default,
            IReadOnlyMetadataContext? metadata = null)
        {
            return GetComponents<IPresenterComponent>(metadata).TryClose(this, request, cancellationToken, metadata);
        }

        #endregion
    }
}