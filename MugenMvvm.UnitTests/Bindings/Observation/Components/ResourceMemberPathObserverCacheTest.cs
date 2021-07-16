using System.Collections.Generic;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Interfaces.Observation;
using MugenMvvm.Bindings.Interfaces.Resources;
using MugenMvvm.Bindings.Observation;
using MugenMvvm.Bindings.Observation.Components;
using MugenMvvm.Bindings.Parsing.Expressions.Binding;
using MugenMvvm.Bindings.Resources;
using MugenMvvm.Extensions;
using MugenMvvm.Tests.Bindings.Observation;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Bindings.Observation.Components
{
    public class ResourceMemberPathObserverCacheTest : UnitTestBase
    {
        public ResourceMemberPathObserverCacheTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            ObservationManager.AddComponent(new ResourceMemberPathObserverCache());
        }

        [Fact]
        public void ShouldCacheInvalidateResourceRequest()
        {
            var resource = new DynamicResource();
            var request = new MemberPathObserverRequest(MemberPath.Get(nameof(IDynamicResource.Value)), default, null, false, false, false,
                new BindingResourceMemberExpressionNode(nameof(resource), "", 0, default, default));

            ObservationManager.AddComponent(new TestMemberPathObserverProviderComponent
            {
                TryGetMemberPathObserver = (om, t, r, m) =>
                {
                    om.ShouldEqual(ObservationManager);
                    t.ShouldEqual(resource);
                    r.ShouldEqual(request);
                    m.ShouldEqual(Metadata);
                    return new TestMemberPathObserver();
                }
            });

            var memberPathObserver1 = ObservationManager.GetMemberPathObserver(resource, request, Metadata);
            var memberPathObserver2 = ObservationManager.GetMemberPathObserver(resource, request, Metadata);
            memberPathObserver1.ShouldEqual(memberPathObserver2, ReferenceEqualityComparer.Instance);
            memberPathObserver1.IsDisposable.ShouldBeFalse();

            ObservationManager.TryInvalidateCache();
            memberPathObserver2 = ObservationManager.GetMemberPathObserver(resource, request, Metadata);
            memberPathObserver1.ShouldNotEqual(memberPathObserver2, ReferenceEqualityComparer.Instance);
            memberPathObserver1.IsDisposable.ShouldBeTrue();
            memberPathObserver2.IsDisposable.ShouldBeFalse();

            memberPathObserver1 = ObservationManager.GetMemberPathObserver(resource, request, Metadata);
            memberPathObserver1.ShouldEqual(memberPathObserver2, ReferenceEqualityComparer.Instance);

            ObservationManager.TryInvalidateCache(nameof(resource));
            memberPathObserver2 = ObservationManager.GetMemberPathObserver(resource, request, Metadata);
            memberPathObserver1.ShouldNotEqual(memberPathObserver2, ReferenceEqualityComparer.Instance);
            memberPathObserver1.IsDisposable.ShouldBeTrue();
            memberPathObserver2.IsDisposable.ShouldBeFalse();
        }

        [Fact]
        public void ShouldNotCacheNonResourceRequest()
        {
            var resource = new object();
            var request = new object();

            ObservationManager.AddComponent(new TestMemberPathObserverProviderComponent
            {
                TryGetMemberPathObserver = (om, t, r, m) =>
                {
                    om.ShouldEqual(ObservationManager);
                    t.ShouldEqual(resource);
                    r.ShouldEqual(request);
                    m.ShouldEqual(Metadata);
                    return new TestMemberPathObserver();
                }
            });
            var memberPathObserver1 = ObservationManager.GetMemberPathObserver(resource, request, Metadata);
            var memberPathObserver2 = ObservationManager.GetMemberPathObserver(resource, request, Metadata);
            memberPathObserver1.ShouldNotEqual(memberPathObserver2, ReferenceEqualityComparer.Instance);
            memberPathObserver1.IsDisposable.ShouldBeTrue();
            memberPathObserver2.IsDisposable.ShouldBeTrue();
        }

        protected override IObservationManager GetObservationManager() => new ObservationManager(ComponentCollectionManager);
    }
}