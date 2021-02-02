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
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Bindings.Observation.Components
{
    public class ResourceMemberPathObserverCacheTest : UnitTestBase
    {
        private readonly ObservationManager _observationManager;

        public ResourceMemberPathObserverCacheTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            _observationManager = new ObservationManager(ComponentCollectionManager);
            _observationManager.AddComponent(new ResourceMemberPathObserverCache());
        }

        [Fact]
        public void ShouldCacheInvalidateResourceRequest()
        {
            var resource = new DynamicResource();
            var request = new MemberPathObserverRequest(MemberPath.Get(nameof(IDynamicResource.Value)), default, null, false, false, false,
                new BindingResourceMemberExpressionNode(nameof(resource), "", 0, default, default));

            _observationManager.AddComponent(new TestMemberPathObserverProviderComponent(_observationManager)
            {
                TryGetMemberPathObserver = (t, r, m) =>
                {
                    t.ShouldEqual(resource);
                    r.ShouldEqual(request);
                    m.ShouldEqual(DefaultMetadata);
                    return new TestMemberPathObserver();
                }
            });

            var memberPathObserver1 = _observationManager.GetMemberPathObserver(resource, request, DefaultMetadata);
            var memberPathObserver2 = _observationManager.GetMemberPathObserver(resource, request, DefaultMetadata);
            memberPathObserver1.ShouldEqual(memberPathObserver2, ReferenceEqualityComparer.Instance);
            memberPathObserver1.IsDisposable.ShouldBeFalse();

            _observationManager.TryInvalidateCache();
            memberPathObserver2 = _observationManager.GetMemberPathObserver(resource, request, DefaultMetadata);
            memberPathObserver1.ShouldNotEqual(memberPathObserver2, ReferenceEqualityComparer.Instance);
            memberPathObserver1.IsDisposable.ShouldBeTrue();
            memberPathObserver2.IsDisposable.ShouldBeFalse();

            memberPathObserver1 = _observationManager.GetMemberPathObserver(resource, request, DefaultMetadata);
            memberPathObserver1.ShouldEqual(memberPathObserver2, ReferenceEqualityComparer.Instance);

            _observationManager.TryInvalidateCache(nameof(resource));
            memberPathObserver2 = _observationManager.GetMemberPathObserver(resource, request, DefaultMetadata);
            memberPathObserver1.ShouldNotEqual(memberPathObserver2, ReferenceEqualityComparer.Instance);
            memberPathObserver1.IsDisposable.ShouldBeTrue();
            memberPathObserver2.IsDisposable.ShouldBeFalse();
        }

        [Fact]
        public void ShouldNotCacheNonResourceRequest()
        {
            var resource = new object();
            var request = new object();

            _observationManager.AddComponent(new TestMemberPathObserverProviderComponent
            {
                TryGetMemberPathObserver = (t, r, m) =>
                {
                    t.ShouldEqual(resource);
                    r.ShouldEqual(request);
                    m.ShouldEqual(DefaultMetadata);
                    return new TestMemberPathObserver();
                }
            });
            var memberPathObserver1 = _observationManager.GetMemberPathObserver(resource, request, DefaultMetadata);
            var memberPathObserver2 = _observationManager.GetMemberPathObserver(resource, request, DefaultMetadata);
            memberPathObserver1.ShouldNotEqual(memberPathObserver2, ReferenceEqualityComparer.Instance);
            memberPathObserver1.IsDisposable.ShouldBeTrue();
            memberPathObserver2.IsDisposable.ShouldBeTrue();
        }
    }
}