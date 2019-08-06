using System.Collections.Generic;
using MugenMvvm.Attributes;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Presenters;
using MugenMvvm.Interfaces.Presenters.Components;
using MugenMvvm.Metadata;

namespace MugenMvvm.Presenters.Components
{
    public class CloseHandlerViewModelPresenterComponent : ICloseablePresenterComponent
    {
        #region Constructors

        [Preserve(Conditional = true)]
        public CloseHandlerViewModelPresenterComponent()
        {
        }

        #endregion

        #region Properties

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        public int GetPriority(object source)
        {
            return Priority;
        }

        public IReadOnlyList<IPresenterResult> TryClose(IMetadataContext metadata)
        {
            var viewModel = metadata.Get(NavigationMetadata.ViewModel);
            if (viewModel == null)
                return Default.EmptyArray<IPresenterResult>();
            var closeHandler = viewModel.Metadata.Get(ViewModelMetadata.CloseHandler);
            if (closeHandler == null)
                return Default.EmptyArray<IPresenterResult>();

            var r = closeHandler(viewModel, metadata);
            if (r == null)
                return Default.EmptyArray<IPresenterResult>();
            return new[] { r };
        }

        #endregion
    }
}