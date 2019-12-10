using System.Collections.Generic;
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
        #region Fields

        private readonly IMetadataContextProvider? _metadataContextProvider;

        #endregion

        #region Constructors

        public Presenter(IComponentCollectionProvider? componentCollectionProvider = null, IMetadataContextProvider? metadataContextProvider = null)
            : base(componentCollectionProvider)
        {
            _metadataContextProvider = metadataContextProvider;
        }

        #endregion

        #region Implementation of interfaces

        public IPresenterResult Show(IReadOnlyMetadataContext metadata)
        {
            var metadataContext = _metadataContextProvider.DefaultIfNull().GetMetadataContext(this, metadata);
            var result = GetComponents<IPresenterComponent>(metadata).TryShow(metadataContext);
            if (result == null)
                ExceptionManager.ThrowPresenterCannotShowRequest(metadata);
            return result;
        }

        public IReadOnlyList<IPresenterResult> TryClose(IReadOnlyMetadataContext metadata)
        {
            var metadataContext = _metadataContextProvider.DefaultIfNull().GetMetadataContext(this, metadata);
            return GetComponents<ICloseablePresenterComponent>(metadata).TryClose(metadataContext) ?? Default.EmptyArray<IPresenterResult>();
        }

        public IReadOnlyList<IPresenterResult> TryRestore(IReadOnlyMetadataContext metadata)
        {
            var metadataContext = _metadataContextProvider.DefaultIfNull().GetMetadataContext(this, metadata);
            return GetComponents<IRestorablePresenterComponent>(metadata).TryRestore(metadataContext) ?? Default.EmptyArray<IPresenterResult>();
        }

        #endregion
    }
}