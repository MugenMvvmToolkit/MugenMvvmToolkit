using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Binding.Observers;
using MugenMvvm.Binding.Observers.MemberPaths;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Binding.Observers
{
    public class MemberPathObserverRequestTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void DefaultShouldBeEmpty()
        {
            default(MemberPathObserverRequest).IsEmpty.ShouldBeTrue();
        }

        [Fact]
        public void ConstructorShouldInitializeValues1()
        {
            var hasStablePath = true;
            var memberFlags = MemberFlags.Attached;
            var observable = true;
            var observableMethodName = "Fds";
            var optional = true;
            IMemberPath path = EmptyMemberPath.Instance;
            var state = new object();

            var request = new MemberPathObserverRequest(path, memberFlags, observableMethodName, hasStablePath, observable, optional, state);
            request.IsEmpty.ShouldBeFalse();
            request.HasStablePath.ShouldEqual(hasStablePath);
            request.MemberFlags.ShouldEqual(memberFlags);
            request.Observable.ShouldEqual(observable);
            request.ObservableMethodName.ShouldEqual(observableMethodName);
            request.Optional.ShouldEqual(optional);
            request.Path.ShouldEqual(path);
            request.State.ShouldEqual(state);
        }

        [Fact]
        public void ConstructorShouldInitializeValues2()
        {
            var hasStablePath = true;
            var memberFlags = MemberFlags.Attached;
            var observable = false;
            var observableMethodName = "Fds";
            var optional = true;
            IMemberPath path = EmptyMemberPath.Instance;
            var state = new object();

            var request = new MemberPathObserverRequest(path, memberFlags, observableMethodName, hasStablePath, observable, optional, state);
            request.IsEmpty.ShouldBeFalse();
            request.HasStablePath.ShouldEqual(hasStablePath);
            request.MemberFlags.ShouldEqual(memberFlags);
            request.Observable.ShouldEqual(observable);
            request.ObservableMethodName.ShouldBeNull();
            request.Optional.ShouldEqual(optional);
            request.Path.ShouldEqual(path);
            request.State.ShouldEqual(state);
        }

        #endregion
    }
}