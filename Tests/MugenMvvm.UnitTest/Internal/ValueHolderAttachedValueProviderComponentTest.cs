using MugenMvvm.Collections;
using MugenMvvm.Interfaces.Internal.Components;
using MugenMvvm.Internal.Components;

namespace MugenMvvm.UnitTest.Internal
{
    public class ValueHolderAttachedValueProviderComponentTest : AttachedValueProviderComponentTestBase
    {
        #region Methods

        protected override object GetSupportedItem()
        {
            return new TestValueHolder<LightDictionary<string, object?>>();
        }

        protected override IAttachedValueProviderComponent GetComponent()
        {
            return new ValueHolderAttachedValueProviderComponent();
        }

        #endregion
    }
}