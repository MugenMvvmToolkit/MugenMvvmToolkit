using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Observers;
using MugenMvvm.Binding.Observers.Components;
using MugenMvvm.Binding.Observers.MemberPaths;
using MugenMvvm.Binding.Observers.PathObservers;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Binding.Observers
{
    public class MemberPathObserverProviderComponentTest : UnitTestBase
    {
        #region Fields

        private const string MethodName = "M";

        #endregion

        #region Methods

        [Fact]
        public void TryGetMemberPathObserverShouldIgnoreUnsupportedRequest()
        {
            new MemberPathObserverProviderComponent().TryGetMemberPathObserver(this, this, DefaultMetadata).ShouldBeNull();
        }

        [Fact]
        public void TryGetMemberPathObserverShouldRespectMemberPathObserverRequestParameters1()
        {
            var path = EmptyMemberPath.Instance;

            var request = new MemberPathObserverRequest(path, MemberFlags.Attached, null, true, true, true);
            var component = new MemberPathObserverProviderComponent();
            var emptyPathObserver = (EmptyPathObserver) component.TryGetMemberPathObserver(this, request, DefaultMetadata)!;
            emptyPathObserver.Target.ShouldEqual(this);


            request = new MemberPathObserverRequest(path, MemberFlags.Attached, MethodName, true, true, true);
            var emptyPathObserverMethod = (MethodEmptyPathObserver) component.TryGetMemberPathObserver(this, request, DefaultMetadata)!;
            emptyPathObserverMethod.Path.ShouldEqual(path);
            emptyPathObserverMethod.Target.ShouldEqual(this);
        }

        [Fact]
        public void TryGetMemberPathObserverShouldRespectMemberPathObserverRequestParameters2()
        {
            var path = new SingleMemberPath("Test");

            var request = new MemberPathObserverRequest(path, MemberFlags.Attached, null, true, true, true);
            var component = new MemberPathObserverProviderComponent();
            var pathObserver = (SinglePathObserver) component.TryGetMemberPathObserver(this, request, DefaultMetadata)!;
            pathObserver.Path.ShouldEqual(path);
            pathObserver.Target.ShouldEqual(this);

            request = new MemberPathObserverRequest(path, MemberFlags.Attached, MethodName, true, true, true);
            var observer = (MethodSinglePathObserver) component.TryGetMemberPathObserver(this, request, DefaultMetadata)!;
            observer.Path.ShouldEqual(path);
            observer.Target.ShouldEqual(this);
        }

        [Fact]
        public void TryGetMemberPathObserverShouldRespectMemberPathObserverRequestParameters3()
        {
            var path = new MultiMemberPath("Test.Test2");

            var request = new MemberPathObserverRequest(path, MemberFlags.Attached, null, true, true, true);
            var component = new MemberPathObserverProviderComponent();
            var pathObserver = (MultiPathObserver) component.TryGetMemberPathObserver(this, request, DefaultMetadata)!;
            pathObserver.Path.ShouldEqual(path);
            pathObserver.Target.ShouldEqual(this);

            request = new MemberPathObserverRequest(path, MemberFlags.Attached, MethodName, true, true, true);
            var observer = (MethodMultiPathObserver) component.TryGetMemberPathObserver(this, request, DefaultMetadata)!;
            observer.Path.ShouldEqual(path);
            observer.Target.ShouldEqual(this);
        }

        #endregion
    }
}