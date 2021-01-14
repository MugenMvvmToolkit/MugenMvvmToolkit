﻿using MugenMvvm.Interfaces.Internal.Components;
using MugenMvvm.Internal.Components;

namespace MugenMvvm.UnitTests.Internal.Components
{
    public class StaticTypeAttachedValueStorageTest : AttachedValueStorageProviderTestBase
    {
        protected override object GetSupportedItem() => typeof(StaticTypeAttachedValueStorageTest);

        protected override IAttachedValueStorageProviderComponent GetComponent() => new StaticTypeAttachedValueStorage();

        public override void ShouldBeEphemeron1()
        {
        }

        public override void ShouldBeEphemeron2()
        {
        }
    }
}