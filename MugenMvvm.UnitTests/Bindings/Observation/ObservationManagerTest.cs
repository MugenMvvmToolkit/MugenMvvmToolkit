using System;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Interfaces.Observation;
using MugenMvvm.Bindings.Observation;
using MugenMvvm.Bindings.Observation.Observers;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Internal;
using MugenMvvm.UnitTests.Bindings.Observation.Internal;
using MugenMvvm.UnitTests.Components;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Bindings.Observation
{
    public class ObservationManagerTest : ComponentOwnerTestBase<IObservationManager>
    {
        [Fact]
        public void GetMemberObserverShouldReturnEmptyObserver()
        {
            var provider = new ObservationManager();
            provider.TryGetMemberObserver(typeof(object), this, DefaultMetadata).IsEmpty.ShouldBeTrue();
        }

        [Fact]
        public void GetMemberPathObserverShouldThrowEmpty()
        {
            var provider = new ObservationManager();
            ShouldThrow<InvalidOperationException>(() => provider.GetMemberPathObserver(this, this, DefaultMetadata));
        }

        [Fact]
        public void GetMemberPathShouldThrowEmpty()
        {
            var provider = new ObservationManager();
            ShouldThrow<InvalidOperationException>(() => provider.GetMemberPath(this, DefaultMetadata));
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void GetMemberPathShouldBeHandledByComponents(int componentCount)
        {
            var provider = new ObservationManager();
            var request = this;
            var result = MemberPath.Empty;
            var invokeCount = 0;
            for (var i = 0; i < componentCount; i++)
            {
                var isLast = i == componentCount - 1;
                var component = new TestMemberPathProviderComponent(provider)
                {
                    Priority = -i,
                    TryGetMemberPath = (o, arg4) =>
                    {
                        ++invokeCount;
                        o.ShouldEqual(request);
                        arg4.ShouldEqual(DefaultMetadata);
                        if (isLast)
                            return result;
                        return null;
                    }
                };
                provider.AddComponent(component);
            }

            provider.GetMemberPath(request, DefaultMetadata).ShouldEqual(result);
            invokeCount.ShouldEqual(componentCount);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void GetMemberObserverShouldBeHandledByComponents(int componentCount)
        {
            var provider = new ObservationManager();
            var type = typeof(string);
            var request = this;
            var result = new MemberObserver((o, o1, arg3, arg4) => ActionToken.NoDoToken, this);
            var invokeCount = 0;
            for (var i = 0; i < componentCount; i++)
            {
                var isLast = i == componentCount - 1;
                var component = new TestMemberObserverProviderComponent(provider)
                {
                    Priority = -i,
                    TryGetMemberObserver = (t, o, arg4) =>
                    {
                        ++invokeCount;
                        t.ShouldEqual(type);
                        o.ShouldEqual(request);
                        arg4.ShouldEqual(DefaultMetadata);
                        if (isLast)
                            return result;
                        return default;
                    }
                };
                provider.AddComponent(component);
            }

            provider.TryGetMemberObserver(type, request, DefaultMetadata).ShouldEqual(result);
            invokeCount.ShouldEqual(componentCount);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void GetMemberPathObserverShouldBeHandledByComponents(int componentCount)
        {
            var provider = new ObservationManager();
            var target = typeof(string);
            var request = this;
            var result = EmptyPathObserver.Empty;
            var invokeCount = 0;
            for (var i = 0; i < componentCount; i++)
            {
                var isLast = i == componentCount - 1;
                var component = new TestMemberPathObserverProviderComponent(provider)
                {
                    Priority = -i,
                    TryGetMemberPathObserver = (t, o, arg4) =>
                    {
                        ++invokeCount;
                        t.ShouldEqual(target);
                        o.ShouldEqual(request);
                        arg4.ShouldEqual(DefaultMetadata);
                        if (isLast)
                            return result;
                        return default;
                    }
                };
                provider.AddComponent(component);
            }

            provider.GetMemberPathObserver(target, request, DefaultMetadata).ShouldEqual(result);
            invokeCount.ShouldEqual(componentCount);
        }

        protected override IObservationManager GetComponentOwner(IComponentCollectionManager? collectionProvider = null) => new ObservationManager(collectionProvider);
    }
}