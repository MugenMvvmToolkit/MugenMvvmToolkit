using MugenMvvm.Interfaces.Internal.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal.Components;
using MugenMvvm.Metadata;
using MugenMvvm.UnitTest.Metadata.Internal;

namespace MugenMvvm.UnitTest.Internal.Components
{
    public class MetadataOwnerAttachedValueProviderTest : AttachedValueProviderTestBase
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

        protected override IAttachedValueProviderComponent GetComponent()
        {
            return new MetadataOwnerAttachedValueProvider();
        }

        #endregion
    }
}