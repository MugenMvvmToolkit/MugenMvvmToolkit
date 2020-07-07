using MugenMvvm.Interfaces.Internal.Components;
using MugenMvvm.Internal.Components;

namespace MugenMvvm.UnitTest.Internal.Components
{
    public class StaticTypeAttachedValueProviderTest : AttachedValueProviderTestBase
    {
        #region Methods

        protected override object GetSupportedItem() => typeof(StaticTypeAttachedValueProviderTest);

        protected override IAttachedValueProviderComponent GetComponent() => new StaticTypeAttachedValueProvider();

        #endregion
    }
}