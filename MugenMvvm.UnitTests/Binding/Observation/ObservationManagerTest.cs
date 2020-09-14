using System;
using MugenMvvm.Binding.Extensions;
using MugenMvvm.Binding.Interfaces.Observation;
using MugenMvvm.Binding.Observation;
using MugenMvvm.Binding.Observation.Observers;
using MugenMvvm.Binding.Observation.Paths;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Internal;
using MugenMvvm.UnitTests.Binding.Observation.Internal;
using MugenMvvm.UnitTests.Components;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Binding.Observation
{
    public class ObservationManagerTest : ComponentOwnerTestBase<IObservationManager>
    {
        #region Methods

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
            var result = EmptyMemberPath.Instance;
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

        [Fact]
        public void GetMemberObserverShouldReturnEmptyObserver()
        {
            var provider = new ObservationManager();
            provider.TryGetMemberObserver(typeof(object), this, DefaultMetadata).IsEmpty.ShouldBeTrue();
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

        [Fact]
        public void GetMemberPathObserverShouldThrowEmpty()
        {
            var provider = new ObservationManager();
            ShouldThrow<InvalidOperationException>(() => provider.GetMemberPathObserver(this, this, DefaultMetadata));
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

        #endregion
    }
}