using MugenMvvm.Interfaces.Internal.Components;
using MugenMvvm.Internal.Components;

namespace MugenMvvm.UnitTest.Internal.Components
{
    public class StaticTypeAttachedValueStorageTest : AttachedValueStorageProviderTestBase
    {
        #region Methods

#if !DEBUG
        public override void ShouldBeEphemeron1()
        {
        }

        public override void ShouldBeEphemeron2()
        {
        }
#endif

        protected override object GetSupportedItem() => typeof(StaticTypeAttachedValueStorageTest);

        protected override IAttachedValueStorageProviderComponent GetComponent() => new StaticTypeAttachedValueStorage();

        #endregion
    }
}