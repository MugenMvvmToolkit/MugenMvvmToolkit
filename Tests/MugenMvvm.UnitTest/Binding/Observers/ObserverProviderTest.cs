using System;
using MugenMvvm.Binding.Extensions;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Binding.Observers;
using MugenMvvm.Binding.Observers.MemberPaths;
using MugenMvvm.Binding.Observers.PathObservers;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Internal;
using MugenMvvm.UnitTest.Binding.Observers.Internal;
using MugenMvvm.UnitTest.Components;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Binding.Observers
{
    public class ObserverProviderTest : ComponentOwnerTestBase<IObserverProvider>
    {
        #region Methods

        [Fact]
        public void GetMemberPathShouldThrowEmpty()
        {
            var provider = new ObserverProvider();
            ShouldThrow<InvalidOperationException>(() => provider.GetMemberPath(this, DefaultMetadata));
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void GetMemberPathShouldBeHandledByComponents(int componentCount)
        {
            var provider = new ObserverProvider();
            var request = this;
            var result = EmptyMemberPath.Instance;
            var invokeCount = 0;
            for (var i = 0; i < componentCount; i++)
            {
                var isLast = i == componentCount - 1;
                var component = new TestMemberPathProviderComponent
                {
                    Priority = -i,
                    TryGetMemberPath = (o, arg3, arg4) =>
                    {
                        ++invokeCount;
                        o.ShouldEqual(request);
                        arg3.ShouldEqual(request.GetType());
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
            var provider = new ObserverProvider();
            provider.TryGetMemberObserver(typeof(object), this, DefaultMetadata).IsEmpty.ShouldBeTrue();
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void GetMemberObserverShouldBeHandledByComponents(int componentCount)
        {
            var provider = new ObserverProvider();
            var type = typeof(string);
            var request = this;
            var result = new MemberObserver((o, o1, arg3, arg4) => ActionToken.NoDoToken, this);
            var invokeCount = 0;
            for (var i = 0; i < componentCount; i++)
            {
                var isLast = i == componentCount - 1;
                var component = new TestMemberObserverProviderComponent
                {
                    Priority = -i,
                    TryGetMemberObserver = (t, o, arg3, arg4) =>
                    {
                        ++invokeCount;
                        t.ShouldEqual(type);
                        o.ShouldEqual(request);
                        arg3.ShouldEqual(request.GetType());
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
            var provider = new ObserverProvider();
            ShouldThrow<InvalidOperationException>(() => provider.GetMemberPathObserver(this, this, DefaultMetadata));
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void GetMemberPathObserverShouldBeHandledByComponents(int componentCount)
        {
            var provider = new ObserverProvider();
            var target = typeof(string);
            var request = this;
            var result = EmptyPathObserver.Empty;
            var invokeCount = 0;
            for (var i = 0; i < componentCount; i++)
            {
                var isLast = i == componentCount - 1;
                var component = new TestMemberPathObserverProviderComponent()
                {
                    Priority = -i,
                    TryGetMemberPathObserver = (t, o, arg3, arg4) =>
                    {
                        ++invokeCount;
                        t.ShouldEqual(target);
                        o.ShouldEqual(request);
                        arg3.ShouldEqual(request.GetType());
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

        protected override IObserverProvider GetComponentOwner(IComponentCollectionProvider? collectionProvider = null)
        {
            return new ObserverProvider(collectionProvider);
        }

        #endregion
    }
}