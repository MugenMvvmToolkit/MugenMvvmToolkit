using System.Collections.Generic;
using System.Threading;
using MugenMvvm.Components;
using MugenMvvm.Extensions;
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

        public PresenterResult Show(IReadOnlyMetadataContext metadata, CancellationToken cancellationToken = default)
        {
            Should.NotBeNull(metadata, nameof(metadata));
            var result = GetComponents<IPresenterComponent>(metadata).TryShow(metadata, cancellationToken);
            if (result.IsEmpty)
                ExceptionManager.ThrowPresenterCannotShowRequest(metadata);
            return result;
        }

        public IReadOnlyList<PresenterResult> TryClose(IReadOnlyMetadataContext metadata, CancellationToken cancellationToken = default)
        {
            Should.NotBeNull(metadata, nameof(metadata));
            return GetComponents<IPresenterComponent>(metadata).TryClose(metadata, cancellationToken) ?? Default.EmptyArray<PresenterResult>();
        }

        public IReadOnlyList<PresenterResult> TryRestore(IReadOnlyMetadataContext metadata, CancellationToken cancellationToken = default)
        {
            Should.NotBeNull(metadata, nameof(metadata));
            return GetComponents<IPresenterComponent>(metadata).TryRestore(metadata, cancellationToken) ?? Default.EmptyArray<PresenterResult>();
        }

        #endregion
    }
}