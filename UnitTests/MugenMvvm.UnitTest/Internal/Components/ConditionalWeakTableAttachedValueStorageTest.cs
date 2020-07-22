using MugenMvvm.Interfaces.Internal.Components;
using MugenMvvm.Internal.Components;

namespace MugenMvvm.UnitTest.Internal.Components
{
    public class ConditionalWeakTableAttachedValueStorageTest : AttachedValueStorageProviderTestBase
    {
        #region Methods

        protected override object GetSupportedItem()
        {
            return new object();
        }

        protected override IAttachedValueStorageProviderComponent GetComponent()
        {
            return new ConditionalWeakTableAttachedValueStorage();
        }

        #endregion
    }
}