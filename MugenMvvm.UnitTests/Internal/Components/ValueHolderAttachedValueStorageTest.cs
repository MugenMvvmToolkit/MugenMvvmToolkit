using System.Collections.Generic;
using MugenMvvm.Interfaces.Internal.Components;
using MugenMvvm.Internal.Components;
using MugenMvvm.Tests.Internal;

namespace MugenMvvm.UnitTests.Internal.Components
{
    public class ValueHolderAttachedValueStorageTest : AttachedValueStorageProviderTestBase
    {
        protected override object GetSupportedItem() => new TestValueHolder<IDictionary<string, object?>>();

        protected override IAttachedValueStorageProviderComponent GetComponent() => new ValueHolderAttachedValueStorage();
    }
}