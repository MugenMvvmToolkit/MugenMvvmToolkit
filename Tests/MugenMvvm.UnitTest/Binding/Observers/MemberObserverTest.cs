using MugenMvvm.Binding.Observers;
using MugenMvvm.Internal;
using MugenMvvm.UnitTest.Binding.Observers.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Binding.Observers
{
    public class MemberObserverTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void DefaultShouldBeEmpty()
        {
            default(MemberObserver).IsEmpty.ShouldBeTrue();
            default(MemberObserver).TryObserve(this, new TestEventListener(), DefaultMetadata).IsEmpty.ShouldBeTrue();
        }

        [Fact]
        public void TryObserveShouldCallDelegate()
        {
            var count = 0;
            var target = new object();
            var listener = new TestEventListener();
            var member = new object();
            var result = ActionToken.NoDoToken;
            var observer = new MemberObserver((t, m, l, meta) =>
            {
                ++count;
                t.ShouldEqual(target);
                m.ShouldEqual(member);
                l.ShouldEqual(listener);
                meta.ShouldEqual(DefaultMetadata);
                return result;
            }, member);

            observer.IsEmpty.ShouldBeFalse();
            observer.Member.ShouldEqual(member);
            observer.TryObserve(target, listener, DefaultMetadata).ShouldEqual(result);
            count.ShouldEqual(1);
        }

        #endregion
    }
}