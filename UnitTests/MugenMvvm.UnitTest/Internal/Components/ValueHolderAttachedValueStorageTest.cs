using System.Collections.Generic;
using MugenMvvm.Interfaces.Internal.Components;
using MugenMvvm.Internal.Components;
using MugenMvvm.UnitTest.Internal.Internal;

namespace MugenMvvm.UnitTest.Internal.Components
{
    public class ValueHolderAttachedValueStorageTest : AttachedValueStorageProviderTestBase
    {
        #region Methods

        protected override object GetSupportedItem() => new TestValueHolder<IDictionary<string, object?>>();

        protected override IAttachedValueStorageProviderComponent GetComponent() => new ValueHolderAttachedValueStorage();

        #endregion
    }
}