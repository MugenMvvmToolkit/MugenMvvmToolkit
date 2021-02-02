using MugenMvvm.Bindings.Enums;
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
        private readonly ObservationManager _observationManager;

        public MemberPathObserverProviderTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            _observationManager = new ObservationManager(ComponentCollectionManager);
            _observationManager.AddComponent(new MemberPathObserverProvider());
        }

        [Fact]
        public void TryGetMemberPathObserverShouldIgnoreUnsupportedRequest() =>
            _observationManager.TryGetMemberPathObserver(this, this, DefaultMetadata).ShouldBeNull();

        [Fact]
        public void TryGetMemberPathObserverShouldRespectMemberPathObserverRequestParameters1()
        {
            var path = MemberPath.Empty;

            var request = new MemberPathObserverRequest(path, MemberFlags.Attached, null, true, true, true, null);
            var emptyPathObserver = (EmptyPathObserver) _observationManager.TryGetMemberPathObserver(this, request, DefaultMetadata)!;
            emptyPathObserver.Target.ShouldEqual(this);

            request = new MemberPathObserverRequest(path, MemberFlags.Attached, MethodName, true, true, true, null);
            var emptyPathObserverMethod = (MethodEmptyPathObserver) _observationManager.TryGetMemberPathObserver(this, request, DefaultMetadata)!;
            emptyPathObserverMethod.Path.ShouldEqual(path);
            emptyPathObserverMethod.Target.ShouldEqual(this);
        }

        [Fact]
        public void TryGetMemberPathObserverShouldRespectMemberPathObserverRequestParameters2()
        {
            var path = MemberPath.Get("Test");

            var request = new MemberPathObserverRequest(path, MemberFlags.Attached, null, true, true, true, null);
            var pathObserver = (SinglePathObserver) _observationManager.TryGetMemberPathObserver(this, request, DefaultMetadata)!;
            pathObserver.Path.ShouldEqual(path);
            pathObserver.Target.ShouldEqual(this);

            request = new MemberPathObserverRequest(path, MemberFlags.Attached, MethodName, true, true, true, null);
            var observer = (MethodSinglePathObserver) _observationManager.TryGetMemberPathObserver(this, request, DefaultMetadata)!;
            observer.Path.ShouldEqual(path);
            observer.Target.ShouldEqual(this);
        }

        [Fact]
        public void TryGetMemberPathObserverShouldRespectMemberPathObserverRequestParameters3()
        {
            var path = MemberPath.Get("Test.Test2");

            var request = new MemberPathObserverRequest(path, MemberFlags.Attached, null, true, true, true, null);
            var pathObserver = (MultiPathObserver) _observationManager.TryGetMemberPathObserver(this, request, DefaultMetadata)!;
            pathObserver.Path.ShouldEqual(path);
            pathObserver.Target.ShouldEqual(this);

            request = new MemberPathObserverRequest(path, MemberFlags.Attached, MethodName, true, true, true, null);
            var observer = (MethodMultiPathObserver) _observationManager.TryGetMemberPathObserver(this, request, DefaultMetadata)!;
            observer.Path.ShouldEqual(path);
            observer.Target.ShouldEqual(this);
        }
    }
}