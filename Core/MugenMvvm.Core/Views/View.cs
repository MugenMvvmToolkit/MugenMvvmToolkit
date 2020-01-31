using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Metadata;

namespace MugenMvvm.Views
{
    public sealed class View : MetadataOwnerBase, IView
    {
        #region Fields

        private readonly object _view;

        #endregion

        #region Constructors

        public View(IViewModelViewMapping mapping, object view, IReadOnlyMetadataContext? metadata = null, IMetadataContextProvider? metadataContextProvider = null)
            : base(metadata, metadataContextProvider)
        {
            Should.NotBeNull(mapping, nameof(mapping));
            Should.NotBeNull(view, nameof(view));
            Mapping = mapping;
            _view = view;
        }

        #endregion

        #region Properties

        public IViewModelViewMapping Mapping { get; }

        object IView.View => _view;

        #endregion
    }
}