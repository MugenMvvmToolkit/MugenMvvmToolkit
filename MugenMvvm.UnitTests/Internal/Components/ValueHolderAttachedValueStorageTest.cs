using System.Collections.Generic;
using MugenMvvm.Interfaces.Internal.Components;
using MugenMvvm.Internal.Components;
using MugenMvvm.UnitTests.Internal.Internal;

namespace MugenMvvm.UnitTests.Internal.Components
{
    public class ValueHolderAttachedValueStorageTest : AttachedValueStorageProviderTestBase
    {
        #region Methods

        protected override object GetSupportedItem() => new TestValueHolder<IDictionary<string, object?>>();

        protected override IAttachedValueStorageProviderComponent GetComponent() => new ValueHolderAttachedValueStorage();

        #endregion
    }
}