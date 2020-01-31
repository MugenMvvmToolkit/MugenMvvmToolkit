using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Metadata
{
    public abstract class MetadataOwnerBase : IMetadataOwner<IMetadataContext>
    {
        #region Fields

        private readonly IMetadataContextProvider? _metadataContextProvider;
        private IReadOnlyMetadataContext? _metadata;

        #endregion

        #region Constructors

        protected MetadataOwnerBase(IReadOnlyMetadataContext? metadata, IMetadataContextProvider? metadataContextProvider)
        {
            _metadata = metadata;
            _metadataContextProvider = metadataContextProvider;
        }

        #endregion

        #region Properties

        public bool HasMetadata => !_metadata.IsNullOrEmpty();

        public IMetadataContext Metadata => _metadataContextProvider.LazyInitializeNonReadonly(ref _metadata, this);

        protected IMetadataContext? MetadataRaw => _metadata as IMetadataContext;

        #endregion
    }
}