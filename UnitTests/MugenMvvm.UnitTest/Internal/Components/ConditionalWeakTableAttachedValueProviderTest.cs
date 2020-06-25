using MugenMvvm.Interfaces.Internal.Components;
using MugenMvvm.Internal.Components;

namespace MugenMvvm.UnitTest.Internal.Components
{
    public class ConditionalWeakTableAttachedValueProviderTest : AttachedValueProviderTestBase
    {
        #region Methods

        protected override object GetSupportedItem()
        {
            return new object();
        }

        protected override IAttachedValueProviderComponent GetComponent()
        {
            return new ConditionalWeakTableAttachedValueProvider();
        }

        #endregion
    }
}