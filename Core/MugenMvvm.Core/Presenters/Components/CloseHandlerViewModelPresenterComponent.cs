using System.Collections.Generic;
using System.Threading;
using MugenMvvm.Attributes;
using MugenMvvm.Constants;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Presenters.Components;
using MugenMvvm.Metadata;

namespace MugenMvvm.Presenters.Components
{
    public sealed class CloseHandlerViewModelPresenterComponent : IPresenterComponent, IHasPriority
    {
        #region Constructors

        [Preserve(Conditional = true)]
        public CloseHandlerViewModelPresenterComponent()
        {
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = PresenterComponentPriority.Default;

        #endregion

        #region Implementation of interfaces

        public PresenterResult TryShow(IReadOnlyMetadataContext metadata, CancellationToken cancellationToken)
        {
            return default;
        }

        public IReadOnlyList<PresenterResult>? TryClose(IReadOnlyMetadataContext metadata, CancellationToken cancellationToken)
        {
            var viewModel = metadata.Get(NavigationMetadata.ViewModel);
            var closeHandler = viewModel?.Metadata.Get(ViewModelMetadata.CloseHandler);
            if (closeHandler == null)
                return null;

            var r = closeHandler(viewModel!, metadata, cancellationToken);
            if (r.IsEmpty)
                return null;
            return new[] { r };
        }

        public IReadOnlyList<PresenterResult>? TryRestore(IReadOnlyMetadataContext metadata, CancellationToken cancellationToken)
        {
            return null;
        }

        #endregion
    }
}