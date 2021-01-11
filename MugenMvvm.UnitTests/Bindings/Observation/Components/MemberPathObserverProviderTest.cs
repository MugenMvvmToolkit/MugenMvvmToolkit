using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Observation;
using MugenMvvm.Bindings.Observation.Components;
using MugenMvvm.Bindings.Observation.Observers;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Bindings.Observation.Components
{
    public class MemberPathObserverProviderTest : UnitTestBase
    {
        #region Fields

        private const string MethodName = "M";

        #endregion

        #region Methods

        [Fact]
        public void TryGetMemberPathObserverShouldIgnoreUnsupportedRequest() => new MemberPathObserverProvider().TryGetMemberPathObserver(null!, this, this, DefaultMetadata).ShouldBeNull();

        [Fact]
        public void TryGetMemberPathObserverShouldRespectMemberPathObserverRequestParameters1()
        {
            var path = MemberPath.Empty;

            var request = new MemberPathObserverRequest(path, MemberFlags.Attached, null, true, true, true, null);
            var component = new MemberPathObserverProvider();
            var emptyPathObserver = (EmptyPathObserver) component.TryGetMemberPathObserver(null!, this, request, DefaultMetadata)!;
            emptyPathObserver.Target.ShouldEqual(this);


            request = new MemberPathObserverRequest(path, MemberFlags.Attached, MethodName, true, true, true, null);
            var emptyPathObserverMethod = (MethodEmptyPathObserver) component.TryGetMemberPathObserver(null!, this, request, DefaultMetadata)!;
            emptyPathObserverMethod.Path.ShouldEqual(path);
            emptyPathObserverMethod.Target.ShouldEqual(this);
        }

        [Fact]
        public void TryGetMemberPathObserverShouldRespectMemberPathObserverRequestParameters2()
        {
            var path = MemberPath.Get("Test");

            var request = new MemberPathObserverRequest(path, MemberFlags.Attached, null, true, true, true, null);
            var component = new MemberPathObserverProvider();
            var pathObserver = (SinglePathObserver) component.TryGetMemberPathObserver(null!, this, request, DefaultMetadata)!;
            pathObserver.Path.ShouldEqual(path);
            pathObserver.Target.ShouldEqual(this);

            request = new MemberPathObserverRequest(path, MemberFlags.Attached, MethodName, true, true, true, null);
            var observer = (MethodSinglePathObserver) component.TryGetMemberPathObserver(null!, this, request, DefaultMetadata)!;
            observer.Path.ShouldEqual(path);
            observer.Target.ShouldEqual(this);
        }

        [Fact]
        public void TryGetMemberPathObserverShouldRespectMemberPathObserverRequestParameters3()
        {
            var path = MemberPath.Get("Test.Test2");

            var request = new MemberPathObserverRequest(path, MemberFlags.Attached, null, true, true, true, null);
            var component = new MemberPathObserverProvider();
            var pathObserver = (MultiPathObserver) component.TryGetMemberPathObserver(null!, this, request, DefaultMetadata)!;
            pathObserver.Path.ShouldEqual(path);
            pathObserver.Target.ShouldEqual(this);

            request = new MemberPathObserverRequest(path, MemberFlags.Attached, MethodName, true, true, true, null);
            var observer = (MethodMultiPathObserver) component.TryGetMemberPathObserver(null!, this, request, DefaultMetadata)!;
            observer.Path.ShouldEqual(path);
            observer.Target.ShouldEqual(this);
        }

        #endregion
    }
}