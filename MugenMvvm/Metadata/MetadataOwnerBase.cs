using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Metadata
{
    public abstract class MetadataOwnerBase : IMetadataOwner<IMetadataContext>
    {
        private IReadOnlyMetadataContext? _metadata;

        protected MetadataOwnerBase(IReadOnlyMetadataContext? metadata)
        {
            _metadata = metadata;
        }

        public bool HasMetadata => !_metadata.IsNullOrEmpty();

        public IMetadataContext Metadata => _metadata as IMetadataContext ?? MugenExtensions.EnsureInitialized(ref _metadata);

        protected IMetadataContext? MetadataRaw => _metadata as IMetadataContext;
    }
}