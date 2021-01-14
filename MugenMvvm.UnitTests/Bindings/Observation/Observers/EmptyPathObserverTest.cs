using MugenMvvm.Bindings.Members;
using MugenMvvm.Bindings.Observation.Observers;
using MugenMvvm.UnitTests.Bindings.Observation.Internal;
using MugenMvvm.UnitTests.Internal.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Bindings.Observation.Observers
{
    public class EmptyPathObserverTest : UnitTestBase
    {
        [Fact]
        public void ConstructorShouldInitializeValues1()
        {
            var o = new object();
            var observer = new EmptyPathObserver(o);
            observer.IsAlive.ShouldBeTrue();
            observer.Target.ShouldEqual(o);
        }

        [Fact]
        public void ConstructorShouldInitializeValues2()
        {
            var o = new TestWeakReference {IsAlive = true, Target = new object()};
            var observer = new EmptyPathObserver(o);
            observer.IsAlive.ShouldBeTrue();
            observer.Target.ShouldEqual(o.Target);

            o.Target = null;
            o.IsAlive = false;
            observer.IsAlive.ShouldBeFalse();
            observer.Target.ShouldBeNull();
        }

        [Fact]
        public void DisposeShouldClearObserver()
        {
            var memberPathObserver = new EmptyPathObserver(this);
            memberPathObserver.IsAlive.ShouldBeTrue();
            memberPathObserver.Target.ShouldEqual(this);

            memberPathObserver.Dispose();
            memberPathObserver.GetLastMember(DefaultMetadata).IsAvailable.ShouldBeFalse();
            memberPathObserver.GetMembers(DefaultMetadata).IsAvailable.ShouldBeFalse();
            memberPathObserver.IsAlive.ShouldBeFalse();
            memberPathObserver.Target.ShouldBeNull();
        }

        [Fact]
        public void GetLastMemberShouldReturnActualMembers()
        {
            var observer = new EmptyPathObserver(this);
            var members = observer.GetLastMember(DefaultMetadata);
            members.Member.ShouldEqual(ConstantMemberInfo.Target);
            members.Target.ShouldEqual(this);
        }

        [Fact]
        public void GetMembersShouldReturnActualMembers()
        {
            var observer = new EmptyPathObserver(this);
            var members = observer.GetMembers(DefaultMetadata);
            members.Members.Item.ShouldEqual(ConstantMemberInfo.Target);
            members.Target.ShouldEqual(this);
        }

        [Fact]
        public void ObserverShouldIgnoreListeners()
        {
            var emptyPathObserver = new EmptyPathObserver(this);
            var listener = new TestMemberPathObserverListener();
            emptyPathObserver.AddListener(listener);
            emptyPathObserver.GetListeners().IsEmpty.ShouldBeTrue();

            emptyPathObserver.RemoveListener(listener);
            emptyPathObserver.GetListeners().IsEmpty.ShouldBeTrue();
        }
    }
}