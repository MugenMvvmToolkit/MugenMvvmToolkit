using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using MugenMvvm.Components;
using MugenMvvm.Extensions;
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

        public Presenter(IComponentCollectionProvider? componentCollectionProvider = null)
            : base(componentCollectionProvider)
        {
        }

        #endregion

        #region Implementation of interfaces

        public ItemOrList<IPresenterResult, IReadOnlyList<IPresenterResult>> Show<TRequest>([DisallowNull] in TRequest request, CancellationToken cancellationToken = default, IReadOnlyMetadataContext? metadata = null)
        {
            var result = GetComponents<IPresenterComponent>(metadata).TryShow(request, cancellationToken, metadata);
            if (result.IsNullOrEmpty())
                ExceptionManager.ThrowPresenterCannotShowRequest(request, metadata);
            return result;
        }

        public ItemOrList<IPresenterResult, IReadOnlyList<IPresenterResult>> TryClose<TRequest>([DisallowNull] in TRequest request, CancellationToken cancellationToken = default,
            IReadOnlyMetadataContext? metadata = null)
        {
            return GetComponents<IPresenterComponent>(metadata).TryClose(request, cancellationToken, metadata);
        }

        #endregion
    }
}