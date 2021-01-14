using MugenMvvm.Interfaces.Internal.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal.Components;
using MugenMvvm.Metadata;
using MugenMvvm.UnitTests.Metadata.Internal;

namespace MugenMvvm.UnitTests.Internal.Components
{
    public class MetadataOwnerAttachedValueStorageTest : AttachedValueStorageProviderTestBase
    {
        protected override object GetSupportedItem() =>
            new TestMetadataOwner<IMetadataContext>
            {
                HasMetadata = true,
                Metadata = new MetadataContext()
            };

        protected override IAttachedValueStorageProviderComponent GetComponent() => new MetadataOwnerAttachedValueStorage();
    }
}