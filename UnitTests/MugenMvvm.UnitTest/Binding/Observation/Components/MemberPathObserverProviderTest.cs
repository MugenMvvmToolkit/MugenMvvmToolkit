using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Observation;
using MugenMvvm.Binding.Observation.Components;
using MugenMvvm.Binding.Observation.Observers;
using MugenMvvm.Binding.Observation.Paths;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Binding.Observation.Components
{
    public class MemberPathObserverProviderTest : UnitTestBase
    {
        #region Fields

        private const string MethodName = "M";

        #endregion

        #region Methods

        [Fact]
        public void TryGetMemberPathObserverShouldIgnoreUnsupportedRequest()
        {
            new MemberPathObserverProvider().TryGetMemberPathObserver(null!, this, this, DefaultMetadata).ShouldBeNull();
        }

        [Fact]
        public void TryGetMemberPathObserverShouldRespectMemberPathObserverRequestParameters1()
        {
            var path = EmptyMemberPath.Instance;

            var request = new MemberPathObserverRequest(path, MemberFlags.Attached, null, true, true, true);
            var component = new MemberPathObserverProvider();
            var emptyPathObserver = (EmptyPathObserver)component.TryGetMemberPathObserver(null!, this, request, DefaultMetadata)!;
            emptyPathObserver.Target.ShouldEqual(this);


            request = new MemberPathObserverRequest(path, MemberFlags.Attached, MethodName, true, true, true);
            var emptyPathObserverMethod = (MethodEmptyPathObserver)component.TryGetMemberPathObserver(null!, this, request, DefaultMetadata)!;
            emptyPathObserverMethod.Path.ShouldEqual(path);
            emptyPathObserverMethod.Target.ShouldEqual(this);
        }

        [Fact]
        public void TryGetMemberPathObserverShouldRespectMemberPathObserverRequestParameters2()
        {
            var path = new SingleMemberPath("Test");

            var request = new MemberPathObserverRequest(path, MemberFlags.Attached, null, true, true, true);
            var component = new MemberPathObserverProvider();
            var pathObserver = (SinglePathObserver)component.TryGetMemberPathObserver(null!, this, request, DefaultMetadata)!;
            pathObserver.Path.ShouldEqual(path);
            pathObserver.Target.ShouldEqual(this);

            request = new MemberPathObserverRequest(path, MemberFlags.Attached, MethodName, true, true, true);
            var observer = (MethodSinglePathObserver)component.TryGetMemberPathObserver(null!, this, request, DefaultMetadata)!;
            observer.Path.ShouldEqual(path);
            observer.Target.ShouldEqual(this);
        }

        [Fact]
        public void TryGetMemberPathObserverShouldRespectMemberPathObserverRequestParameters3()
        {
            var path = new MultiMemberPath("Test.Test2");

            var request = new MemberPathObserverRequest(path, MemberFlags.Attached, null, true, true, true);
            var component = new MemberPathObserverProvider();
            var pathObserver = (MultiPathObserver)component.TryGetMemberPathObserver(null!, this, request, DefaultMetadata)!;
            pathObserver.Path.ShouldEqual(path);
            pathObserver.Target.ShouldEqual(this);

            request = new MemberPathObserverRequest(path, MemberFlags.Attached, MethodName, true, true, true);
            var observer = (MethodMultiPathObserver)component.TryGetMemberPathObserver(null!, this, request, DefaultMetadata)!;
            observer.Path.ShouldEqual(path);
            observer.Target.ShouldEqual(this);
        }

        #endregion
    }
}