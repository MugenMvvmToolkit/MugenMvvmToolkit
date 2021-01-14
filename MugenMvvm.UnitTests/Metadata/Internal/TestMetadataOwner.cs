using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.UnitTests.Metadata.Internal
{
    public class TestMetadataOwner<T> : IMetadataOwner<T> where T : class, IReadOnlyMetadataContext
    {
        public bool HasMetadata { get; set; }

        public T Metadata { get; set; } = null!;
    }
}