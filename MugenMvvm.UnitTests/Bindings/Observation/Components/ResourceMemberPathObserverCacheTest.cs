using System.Collections.Generic;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Interfaces.Resources;
using MugenMvvm.Bindings.Observation;
using MugenMvvm.Bindings.Observation.Components;
using MugenMvvm.Bindings.Parsing.Expressions.Binding;
using MugenMvvm.Bindings.Resources;
using MugenMvvm.Extensions;
using MugenMvvm.UnitTests.Bindings.Observation.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Bindings.Observation.Components
{
    public class ResourceMemberPathObserverCacheTest : UnitTestBase
    {
        [Fact]
        public void ShouldCacheInvalidateResourceRequest()
        {
            var resource = new DynamicResource();
            var request = new MemberPathObserverRequest(MemberPath.Get(nameof(IDynamicResource.Value)), default, null, false, false, false,
                new BindingResourceMemberExpressionNode(nameof(resource), "", 0, default, default));

            var observationManager = new ObservationManager();
            observationManager.AddComponent(new ResourceMemberPathObserverCache());
            observationManager.AddComponent(new TestMemberPathObserverProviderComponent(observationManager)
            {
                TryGetMemberPathObserver = (t, r, m) =>
                {
                    t.ShouldEqual(resource);
                    r.ShouldEqual(request);
                    m.ShouldEqual(DefaultMetadata);
                    return new TestMemberPathObserver();
                }
            });

            var memberPathObserver1 = observationManager.GetMemberPathObserver(resource, request, DefaultMetadata);
            var memberPathObserver2 = observationManager.GetMemberPathObserver(resource, request, DefaultMetadata);
            memberPathObserver1.ShouldEqual(memberPathObserver2, ReferenceEqualityComparer.Instance);
            memberPathObserver1.IsDisposable.ShouldBeFalse();

            observationManager.TryInvalidateCache();
            memberPathObserver2 = observationManager.GetMemberPathObserver(resource, request, DefaultMetadata);
            memberPathObserver1.ShouldNotEqual(memberPathObserver2, ReferenceEqualityComparer.Instance);
            memberPathObserver1.IsDisposable.ShouldBeTrue();
            memberPathObserver2.IsDisposable.ShouldBeFalse();

            memberPathObserver1 = observationManager.GetMemberPathObserver(resource, request, DefaultMetadata);
            memberPathObserver1.ShouldEqual(memberPathObserver2, ReferenceEqualityComparer.Instance);

            observationManager.TryInvalidateCache(nameof(resource));
            memberPathObserver2 = observationManager.GetMemberPathObserver(resource, request, DefaultMetadata);
            memberPathObserver1.ShouldNotEqual(memberPathObserver2, ReferenceEqualityComparer.Instance);
            memberPathObserver1.IsDisposable.ShouldBeTrue();
            memberPathObserver2.IsDisposable.ShouldBeFalse();
        }

        [Fact]
        public void ShouldNotCacheNonResourceRequest()
        {
            var resource = new object();
            var request = new object();

            var observationManager = new ObservationManager();
            observationManager.AddComponent(new ResourceMemberPathObserverCache());
            observationManager.AddComponent(new TestMemberPathObserverProviderComponent
            {
                TryGetMemberPathObserver = (t, r, m) =>
                {
                    t.ShouldEqual(resource);
                    r.ShouldEqual(request);
                    m.ShouldEqual(DefaultMetadata);
                    return new TestMemberPathObserver();
                }
            });
            var memberPathObserver1 = observationManager.GetMemberPathObserver(resource, request, DefaultMetadata);
            var memberPathObserver2 = observationManager.GetMemberPathObserver(resource, request, DefaultMetadata);
            memberPathObserver1.ShouldNotEqual(memberPathObserver2, ReferenceEqualityComparer.Instance);
            memberPathObserver1.IsDisposable.ShouldBeTrue();
            memberPathObserver2.IsDisposable.ShouldBeTrue();
        }
    }
}