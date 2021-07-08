using System;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Interfaces.Observation;
using MugenMvvm.Bindings.Observation;
using MugenMvvm.Bindings.Observation.Observers;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Internal;
using MugenMvvm.Tests.Bindings.Observation;
using MugenMvvm.UnitTests.Components;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Bindings.Observation
{
    public class ObservationManagerTest : ComponentOwnerTestBase<IObservationManager>
    {
        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void GetMemberObserverShouldBeHandledByComponents(int componentCount)
        {
            var type = typeof(string);
            var request = this;
            var result = new MemberObserver((o, o1, arg3, arg4) => ActionToken.NoDo, this);
            var invokeCount = 0;
            for (var i = 0; i < componentCount; i++)
            {
                var isLast = i == componentCount - 1;
                var component = new TestMemberObserverProviderComponent
                {
                    Priority = -i,
                    TryGetMemberObserver = (om, t, o, arg4) =>
                    {
                        ++invokeCount;
                        om.ShouldEqual(ObservationManager);
                        t.ShouldEqual(type);
                        o.ShouldEqual(request);
                        arg4.ShouldEqual(DefaultMetadata);
                        if (isLast)
                            return result;
                        return default;
                    }
                };
                ObservationManager.AddComponent(component);
            }

            ObservationManager.TryGetMemberObserver(type, request, DefaultMetadata).ShouldEqual(result);
            invokeCount.ShouldEqual(componentCount);
        }

        [Fact]
        public void GetMemberObserverShouldReturnEmptyObserver() => ObservationManager.TryGetMemberObserver(typeof(object), this, DefaultMetadata).IsEmpty.ShouldBeTrue();

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void GetMemberPathObserverShouldBeHandledByComponents(int componentCount)
        {
            var target = typeof(string);
            var request = this;
            var result = EmptyPathObserver.Empty;
            var invokeCount = 0;
            for (var i = 0; i < componentCount; i++)
            {
                var isLast = i == componentCount - 1;
                var component = new TestMemberPathObserverProviderComponent
                {
                    Priority = -i,
                    TryGetMemberPathObserver = (om, t, o, arg4) =>
                    {
                        ++invokeCount;
                        om.ShouldEqual(ObservationManager);
                        t.ShouldEqual(target);
                        o.ShouldEqual(request);
                        arg4.ShouldEqual(DefaultMetadata);
                        if (isLast)
                            return result;
                        return default;
                    }
                };
                ObservationManager.AddComponent(component);
            }

            ObservationManager.GetMemberPathObserver(target, request, DefaultMetadata).ShouldEqual(result);
            invokeCount.ShouldEqual(componentCount);
        }

        [Fact]
        public void GetMemberPathObserverShouldThrowEmpty() => ShouldThrow<InvalidOperationException>(() => ObservationManager.GetMemberPathObserver(this, this, DefaultMetadata));

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void GetMemberPathShouldBeHandledByComponents(int componentCount)
        {
            var request = this;
            var result = MemberPath.Empty;
            var invokeCount = 0;
            for (var i = 0; i < componentCount; i++)
            {
                var isLast = i == componentCount - 1;
                var component = new TestMemberPathProviderComponent
                {
                    Priority = -i,
                    TryGetMemberPath = (om, o, arg4) =>
                    {
                        ++invokeCount;
                        om.ShouldEqual(ObservationManager);
                        o.ShouldEqual(request);
                        arg4.ShouldEqual(DefaultMetadata);
                        if (isLast)
                            return result;
                        return null;
                    }
                };
                ObservationManager.AddComponent(component);
            }

            ObservationManager.GetMemberPath(request, DefaultMetadata).ShouldEqual(result);
            invokeCount.ShouldEqual(componentCount);
        }

        [Fact]
        public void GetMemberPathShouldThrowEmpty() => ShouldThrow<InvalidOperationException>(() => ObservationManager.GetMemberPath(this, DefaultMetadata));

        protected override IObservationManager GetObservationManager() => GetComponentOwner(ComponentCollectionManager);

        protected override IObservationManager GetComponentOwner(IComponentCollectionManager? componentCollectionManager = null) =>
            new ObservationManager(componentCollectionManager);
    }
}