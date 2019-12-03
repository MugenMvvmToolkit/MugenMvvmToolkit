using System.Collections.Generic;
using MugenMvvm.Components;
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
            var components = GetComponents<IPresenterComponent>(metadata);
            for (var i = 0; i < components.Length; i++)
            {
                var result = components[i].TryShow(metadataContext);
                if (result != null)
                    return result;
            }

            ExceptionManager.ThrowPresenterCannotShowRequest(metadata);
            return null;
        }

        public IReadOnlyList<IPresenterResult> TryClose(IReadOnlyMetadataContext metadata)
        {
            var metadataContext = _metadataContextProvider.DefaultIfNull().GetMetadataContext(this, metadata);
            var components = GetComponents<ICloseablePresenterComponent>(metadata);
            var results = new List<IPresenterResult>();
            for (var i = 0; i < components.Length; i++)
            {
                var operations = components[i].TryClose(metadataContext);
                if (operations != null)
                    results.AddRange(operations);
            }

            return results;
        }

        public IReadOnlyList<IPresenterResult> TryRestore(IReadOnlyMetadataContext metadata)
        {
            var metadataContext = _metadataContextProvider.DefaultIfNull().GetMetadataContext(this, metadata);
            var components = GetComponents<IRestorablePresenterComponent>(metadata);
            var results = new List<IPresenterResult>();
            for (var i = 0; i < components.Length; i++)
            {
                var operations = components[i].TryRestore(metadataContext);
                if (operations != null)
                    results.AddRange(operations);
            }

            return results;
        }

        #endregion
    }
}