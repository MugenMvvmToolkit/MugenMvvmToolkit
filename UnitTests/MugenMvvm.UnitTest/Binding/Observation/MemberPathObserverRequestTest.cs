using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Observation;
using MugenMvvm.Binding.Observation;
using MugenMvvm.Binding.Observation.Paths;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Binding.Observation
{
    public class MemberPathObserverRequestTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void ConstructorShouldInitializeValues1()
        {
            var hasStablePath = true;
            var memberFlags = MemberFlags.Attached;
            var observable = true;
            var observableMethodName = "Fds";
            var optional = true;
            IMemberPath path = EmptyMemberPath.Instance;

            var request = new MemberPathObserverRequest(path, memberFlags, observableMethodName, hasStablePath, observable, optional);
            request.HasStablePath.ShouldEqual(hasStablePath);
            request.MemberFlags.ShouldEqual(memberFlags);
            request.Observable.ShouldEqual(observable);
            request.ObservableMethodName.ShouldEqual(observableMethodName);
            request.Optional.ShouldEqual(optional);
            request.Path.ShouldEqual(path);
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

            var request = new MemberPathObserverRequest(path, memberFlags, observableMethodName, hasStablePath, observable, optional);
            request.HasStablePath.ShouldEqual(hasStablePath);
            request.MemberFlags.ShouldEqual(memberFlags);
            request.Observable.ShouldEqual(observable);
            request.ObservableMethodName.ShouldBeNull();
            request.Optional.ShouldEqual(optional);
            request.Path.ShouldEqual(path);
        }

        #endregion
    }
}