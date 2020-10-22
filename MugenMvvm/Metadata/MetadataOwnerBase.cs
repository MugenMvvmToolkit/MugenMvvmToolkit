using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Metadata
{
    public abstract class MetadataOwnerBase : IMetadataOwner<IMetadataContext>
    {
        #region Fields

        private IReadOnlyMetadataContext? _metadata;

        #endregion

        #region Constructors

        protected MetadataOwnerBase(IReadOnlyMetadataContext? metadata)
        {
            _metadata = metadata;
        }

        #endregion

        #region Properties

        public bool HasMetadata => !_metadata.IsNullOrEmpty();

        public IMetadataContext Metadata => _metadata as IMetadataContext ?? MugenExtensions.EnsureInitialized(ref _metadata);

        protected IMetadataContext? MetadataRaw => _metadata as IMetadataContext;

        #endregion
    }
}