using MugenMvvm.Interfaces.Internal.Components;
using MugenMvvm.Internal.Components;

namespace MugenMvvm.UnitTest.Internal.Components
{
    public class StaticTypeAttachedValueStorageTest : AttachedValueStorageProviderTestBase
    {
        #region Methods

        protected override object GetSupportedItem() => typeof(StaticTypeAttachedValueStorageTest);

        protected override IAttachedValueStorageProviderComponent GetComponent() => new StaticTypeAttachedValueStorage();

        #endregion
    }
}