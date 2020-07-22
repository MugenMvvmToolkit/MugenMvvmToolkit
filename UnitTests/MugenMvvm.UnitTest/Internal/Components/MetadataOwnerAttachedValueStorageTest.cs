using MugenMvvm.Interfaces.Internal.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal.Components;
using MugenMvvm.Metadata;
using MugenMvvm.UnitTest.Metadata.Internal;

namespace MugenMvvm.UnitTest.Internal.Components
{
    public class MetadataOwnerAttachedValueStorageTest : AttachedValueStorageProviderTestBase
    {
        #region Methods

        protected override object GetSupportedItem()
        {
            return new TestMetadataOwner<IMetadataContext>
            {
                HasMetadata = true,
                Metadata = new MetadataContext()
            };
        }

        protected override IAttachedValueStorageProviderComponent GetComponent()
        {
            return new MetadataOwnerAttachedValueStorage();
        }

        #endregion
    }
}