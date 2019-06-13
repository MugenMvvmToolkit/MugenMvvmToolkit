using System.Collections.Generic;
using MugenMvvm.Attributes;
using MugenMvvm.Constants;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Navigation.Presenters;
using MugenMvvm.Metadata;

namespace MugenMvvm.Infrastructure.Navigation.Presenters
{
    public class CloseHandlerViewModelPresenter : IChildViewModelPresenter
    {
        #region Constructors

        [Preserve(Conditional = true)]
        public CloseHandlerViewModelPresenter()
        {
        }

        #endregion

        #region Properties

        public int Priority => PresenterConstants.CloseHandlerPresenterPriority;

        #endregion

        #region Implementation of interfaces

        public IChildViewModelPresenterResult? TryShow(IViewModelPresenter parentPresenter, IReadOnlyMetadataContext metadata)
        {
            return null;
        }

        public IReadOnlyList<IChildViewModelPresenterResult> TryClose(IViewModelPresenter parentPresenter, IReadOnlyMetadataContext metadata)
        {
            var viewModel = metadata.Get(NavigationMetadata.ViewModel);
            if (viewModel == null)
                return Default.EmptyArray<IChildViewModelPresenterResult>();
            var closeHandler = viewModel.Metadata.Get(ViewModelMetadata.CloseHandler);
            if (closeHandler == null)
                return Default.EmptyArray<IChildViewModelPresenterResult>();

            var r = closeHandler(viewModel, metadata);
            if (r == null)
                return Default.EmptyArray<IChildViewModelPresenterResult>();
            return new[] {r};
        }

        #endregion
    }
}