using MugenMvvm.Interfaces.Internal.Components;
using MugenMvvm.Internal.Components;

namespace MugenMvvm.UnitTest.Internal
{
    public class ConditionalWeakTableAttachedValueProviderComponentTest : AttachedValueProviderComponentTestBase
    {
        #region Methods

        protected override object GetSupportedItem()
        {
            return new object();
        }

        protected override IAttachedValueProviderComponent GetComponent()
        {
            return new ConditionalWeakTableAttachedValueProviderComponent();
        }

        #endregion
    }
}