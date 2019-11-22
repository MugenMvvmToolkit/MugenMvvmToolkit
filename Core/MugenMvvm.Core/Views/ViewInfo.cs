using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Views;

namespace MugenMvvm.Views
{
    public sealed class ViewInfo : IViewInfo
    {
        #region Fields

        private readonly IMetadataContextProvider? _metadataContextProvider;
        private IMetadataContext? _metadata;

        #endregion

        #region Constructors

        public ViewInfo(IViewInitializer initializer, object view, IMetadataContext? metadata = null, IMetadataContextProvider? metadataContextProvider = null)
        {
            Should.NotBeNull(initializer, nameof(initializer));
            Should.NotBeNull(view, nameof(view));
            _metadata = metadata;
            _metadataContextProvider = metadataContextProvider;
            Initializer = initializer;
            View = view;
        }

        #endregion

        #region Properties

        public IMetadataContextProvider MetadataContextProvider => _metadataContextProvider.DefaultIfNull();

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

        public IViewInitializer Initializer { get; }

        public object View { get; }

        #endregion
    }
}