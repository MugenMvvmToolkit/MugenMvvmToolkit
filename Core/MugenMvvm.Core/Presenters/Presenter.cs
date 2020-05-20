using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using MugenMvvm.Components;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Presenters;
using MugenMvvm.Interfaces.Presenters.Components;

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

        public IPresenterResult Show<TRequest>([DisallowNull] in TRequest request, IReadOnlyMetadataContext? metadata = null, CancellationToken cancellationToken = default)
        {
            var result = GetComponents<IPresenterComponent>(metadata).TryShow(request, metadata, cancellationToken);
            if (result == null)
                ExceptionManager.ThrowPresenterCannotShowRequest(request, metadata);
            return result;
        }

        public IReadOnlyList<IPresenterResult> TryClose<TRequest>([DisallowNull] in TRequest request, IReadOnlyMetadataContext? metadata = null, CancellationToken cancellationToken = default)
        {
            return GetComponents<IPresenterComponent>(metadata).TryClose(request, metadata, cancellationToken) ?? Default.EmptyArray<IPresenterResult>();
        }

        public IReadOnlyList<IPresenterResult> TryRestore<TRequest>([DisallowNull] in TRequest request, IReadOnlyMetadataContext? metadata = null, CancellationToken cancellationToken = default)
        {
            return GetComponents<IPresenterComponent>(metadata).TryRestore(request, metadata, cancellationToken) ?? Default.EmptyArray<IPresenterResult>();
        }

        #endregion
    }
}