using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Interfaces.Observation;
using MugenMvvm.Bindings.Observation;
using MugenMvvm.Bindings.Observation.Components;
using MugenMvvm.Bindings.Observation.Observers;
using MugenMvvm.Extensions;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Bindings.Observation.Components
{
    public class MemberPathObserverProviderTest : UnitTestBase
    {
        private const string MethodName = "M";

        public MemberPathObserverProviderTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            ObservationManager.AddComponent(new MemberPathObserverProvider());
        }

        [Fact]
        public void TryGetMemberPathObserverShouldIgnoreUnsupportedRequest() =>
            ObservationManager.TryGetMemberPathObserver(this, this, DefaultMetadata).ShouldBeNull();

        [Fact]
        public void TryGetMemberPathObserverShouldRespectMemberPathObserverRequestParameters1()
        {
            var path = MemberPath.Empty;

            var request = new MemberPathObserverRequest(path, MemberFlags.Attached, null, true, true, true, null);
            var emptyPathObserver = (EmptyPathObserver)ObservationManager.TryGetMemberPathObserver(this, request, DefaultMetadata)!;
            emptyPathObserver.Target.ShouldEqual(this);

            request = new MemberPathObserverRequest(path, MemberFlags.Attached, MethodName, true, true, true, null);
            var emptyPathObserverMethod = (MethodEmptyPathObserver)ObservationManager.TryGetMemberPathObserver(this, request, DefaultMetadata)!;
            emptyPathObserverMethod.Path.ShouldEqual(path);
            emptyPathObserverMethod.Target.ShouldEqual(this);
        }

        [Fact]
        public void TryGetMemberPathObserverShouldRespectMemberPathObserverRequestParameters2()
        {
            var path = MemberPath.Get("Test");

            var request = new MemberPathObserverRequest(path, MemberFlags.Attached, null, true, true, true, null);
            var pathObserver = (SinglePathObserver)ObservationManager.TryGetMemberPathObserver(this, request, DefaultMetadata)!;
            pathObserver.Path.ShouldEqual(path);
            pathObserver.Target.ShouldEqual(this);

            request = new MemberPathObserverRequest(path, MemberFlags.Attached, MethodName, true, true, true, null);
            var observer = (MethodSinglePathObserver)ObservationManager.TryGetMemberPathObserver(this, request, DefaultMetadata)!;
            observer.Path.ShouldEqual(path);
            observer.Target.ShouldEqual(this);
        }

        [Fact]
        public void TryGetMemberPathObserverShouldRespectMemberPathObserverRequestParameters3()
        {
            var path = MemberPath.Get("Test.Test2");

            var request = new MemberPathObserverRequest(path, MemberFlags.Attached, null, true, true, true, null);
            var pathObserver = (MultiPathObserver)ObservationManager.TryGetMemberPathObserver(this, request, DefaultMetadata)!;
            pathObserver.Path.ShouldEqual(path);
            pathObserver.Target.ShouldEqual(this);

            request = new MemberPathObserverRequest(path, MemberFlags.Attached, MethodName, true, true, true, null);
            var observer = (MethodMultiPathObserver)ObservationManager.TryGetMemberPathObserver(this, request, DefaultMetadata)!;
            observer.Path.ShouldEqual(path);
            observer.Target.ShouldEqual(this);
        }

        protected override IObservationManager GetObservationManager() => new ObservationManager(ComponentCollectionManager);
    }
}