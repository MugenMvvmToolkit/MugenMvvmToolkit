using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Metadata
{
    public abstract class MetadataOwnerBase : IMetadataOwner<IMetadataContext>
    {
        #region Fields

        private readonly IMetadataContextManager? _metadataContextManager;
        private IReadOnlyMetadataContext? _metadata;

        #endregion

        #region Constructors

        protected MetadataOwnerBase(IReadOnlyMetadataContext? metadata, IMetadataContextManager? metadataContextManager)
        {
            _metadata = metadata;
            _metadataContextManager = metadataContextManager;
        }

        #endregion

        #region Properties

        public bool HasMetadata => !_metadata.IsNullOrEmpty();

        public IMetadataContext Metadata => _metadataContextManager.LazyInitializeNonReadonly(ref _metadata, this);

        protected IMetadataContext? MetadataRaw => _metadata as IMetadataContext;

        #endregion
    }
}