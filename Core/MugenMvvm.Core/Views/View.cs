using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Views;

namespace MugenMvvm.Views
{
    public sealed class View : IView
    {
        #region Fields

        private readonly IMetadataContextProvider? _metadataContextProvider;
        private readonly object _view;
        private IMetadataContext? _metadata;

        #endregion

        #region Constructors

        public View(IViewModelViewMapping mapping, object view, IMetadataContext? metadata = null, IMetadataContextProvider? metadataContextProvider = null)
        {
            Should.NotBeNull(mapping, nameof(mapping));
            Should.NotBeNull(view, nameof(view));
            _metadata = metadata;
            _metadataContextProvider = metadataContextProvider;
            Mapping = mapping;
            _view = view;
        }

        #endregion

        #region Properties

        public bool HasMetadata => !_metadata.IsNullOrEmpty();

        public IMetadataContext Metadata
        {
            get
            {
                if (_metadata == null)
                    _metadataContextProvider.LazyInitialize(ref _metadata, this);
                return _metadata;
            }
        }

        public IViewModelViewMapping Mapping { get; }

        object IView.View => _view;

        #endregion
    }
}