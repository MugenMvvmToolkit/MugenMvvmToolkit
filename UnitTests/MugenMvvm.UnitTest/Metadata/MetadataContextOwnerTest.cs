using MugenMvvm.Interfaces.Components;
using MugenMvvm.Metadata;
using MugenMvvm.UnitTest.Components;

namespace MugenMvvm.UnitTest.Metadata
{
    public class MetadataContextOwnerTest : ComponentOwnerTestBase<MetadataContext>
    {
        #region Methods

        public override void ComponentOwnerShouldUseCollectionFactory(bool globalValue)
        {
            if (globalValue)
                base.ComponentOwnerShouldUseCollectionFactory(true);
        }

        protected override MetadataContext GetComponentOwner(IComponentCollectionManager? collectionProvider = null) => new MetadataContext();

        #endregion
    }
}