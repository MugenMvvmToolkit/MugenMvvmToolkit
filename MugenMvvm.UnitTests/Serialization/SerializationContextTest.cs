using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Serialization;
using MugenMvvm.UnitTests.Metadata;

namespace MugenMvvm.UnitTests.Serialization
{
    public class SerializationContextTest : MetadataOwnerTestBase
    {
        protected override IMetadataOwner<IMetadataContext> GetMetadataOwner(IReadOnlyMetadataContext? metadata)
            => new SerializationContext<object?, object?>(new SerializationFormat<object?, object?>(1, ""), null, metadata);
    }
}