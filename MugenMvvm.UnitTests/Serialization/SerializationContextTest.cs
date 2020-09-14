using System.IO;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Serialization;
using MugenMvvm.UnitTests.Metadata;

namespace MugenMvvm.UnitTests.Serialization
{
    public class SerializationContextTest : MetadataOwnerTestBase
    {
        #region Methods

        protected override IMetadataOwner<IMetadataContext> GetMetadataOwner(IReadOnlyMetadataContext? metadata) => new SerializationContext(Stream.Null, true, metadata);

        #endregion
    }
}