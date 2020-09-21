using System;
using MugenMvvm.Internal;
using MugenMvvm.UnitTests.Internal.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Internal
{
    public class AttachedValueStorageTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void ShouldThrowEmpty()
        {
            default(AttachedValueStorage).IsEmpty.ShouldBeTrue();
            ShouldThrow<InvalidOperationException>(() => new AttachedValueStorage().GetCount());
            ShouldThrow<InvalidOperationException>(() => new AttachedValueStorage().Contains(""));
            ShouldThrow<InvalidOperationException>(() => new AttachedValueStorage().TryGet("", out _));
            ShouldThrow<InvalidOperationException>(() => new AttachedValueStorage().GetValues(""));
            ShouldThrow<InvalidOperationException>(() => new AttachedValueStorage().Set("", null, out _));
            ShouldThrow<InvalidOperationException>(() => new AttachedValueStorage().Remove("", out _));
            ShouldThrow<InvalidOperationException>(() => new AttachedValueStorage().AddOrUpdate("", "", "", (item, value, currentValue, state) => ""));
            ShouldThrow<InvalidOperationException>(() => new AttachedValueStorage().AddOrUpdate("", "", (o, s) => "", (item, value, currentValue, state) => ""));
            ShouldThrow<InvalidOperationException>(() => new AttachedValueStorage().GetOrAdd("", ""));
            ShouldThrow<InvalidOperationException>(() => new AttachedValueStorage().GetOrAdd("", "", (o, s) => ""));
            ShouldThrow<InvalidOperationException>(() => new AttachedValueStorage().Clear());
            ShouldThrow<InvalidOperationException>(() => new AttachedValueStorage().GetInternalState(out _, out _, out _));
        }

        [Fact]
        public void GetInternalStateShouldReturnInternalState()
        {
            var item = new object();
            var state = "";
            var manager = new TestAttachedValueStorageManager();
            var attachedValueStorage = new AttachedValueStorage(item, manager, state);

            attachedValueStorage.GetInternalState(out var internalManager, out var internalItem, out var internalState);
            internalManager.ShouldEqual(manager);
            internalItem.ShouldEqual(item);
            internalState.ShouldEqual(state);
        }

        #endregion
    }
}