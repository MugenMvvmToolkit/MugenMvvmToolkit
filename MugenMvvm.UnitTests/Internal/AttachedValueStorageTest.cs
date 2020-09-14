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
            ShouldThrow<InvalidOperationException>(() => new AttachedValueStorage().Decorate("", (item, manager, state, s) => default));
        }

        [Fact]
        public void DecorateShouldUseDelegate()
        {
            var item = new object();
            var state = 1;
            var internalState = "";
            var manager = new TestAttachedValueStorageManager();
            var attachedValueStorage = new AttachedValueStorage(item, manager, internalState);

            var newManager = new TestAttachedValueStorageManager
            {
                GetCount = (o, o1) =>
                {
                    o.ShouldEqual(item);
                    o1.ShouldEqual(internalState);
                    return int.MaxValue;
                }
            };
            var invokeCount = 0;
            attachedValueStorage = attachedValueStorage.Decorate(state, (o, storageManager, state1, i) =>
            {
                ++invokeCount;
                o.ShouldEqual(item);
                storageManager.ShouldEqual(manager);
                state1.ShouldEqual(internalState);
                i.ShouldEqual(state);
                return new AttachedValueStorage(item, newManager, internalState);
            });
            attachedValueStorage.GetCount().ShouldEqual(int.MaxValue);
            invokeCount.ShouldEqual(1);
        }

        #endregion
    }
}