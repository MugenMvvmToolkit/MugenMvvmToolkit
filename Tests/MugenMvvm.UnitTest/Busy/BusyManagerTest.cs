using System;
using System.Collections.Generic;
using MugenMvvm.Busy;
using MugenMvvm.Delegates;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Busy;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.UnitTest.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Busy
{
    public class BusyManagerTest : SuspendableComponentOwnerTestBase<BusyManager>
    {
        #region Methods

        [Fact]
        public void ShouldValidateInputArgs()
        {
            var componentOwner = GetComponentOwner();
            ShouldThrow<ArgumentNullException>(() => componentOwner.TryGetToken(componentOwner, null!));
        }

        [Fact]
        public void BeginBusyShouldThrowNoComponents()
        {
            var componentOwner = GetComponentOwner();
            ShouldThrow<InvalidOperationException>(() => componentOwner.BeginBusy(componentOwner));
        }

        [Fact]
        public void TryGetTokenShouldReturnNullNoComponents()
        {
            var componentOwner = GetComponentOwner();
            componentOwner.TryGetToken(componentOwner, (manager, token, arg3) => true).ShouldBeNull();
        }

        [Fact]
        public void GetTokensShouldReturnEmptyNoComponents()
        {
            var componentOwner = GetComponentOwner();
            componentOwner.GetTokens().ShouldBeEmpty();
        }

        [Theory]
        [InlineData(1)]
        [InlineData(100)]
        public void BeginBusyShouldBeHandledByComponents(int componentCount)
        {
            var componentOwner = GetComponentOwner();
            var busyToken = new TestBusyToken();
            var methodCallCount = 0;
            for (var i = 0; i < componentCount; i++)
            {
                var component = new TestBusyManagerComponent();
                var i1 = i;
                component.Priority = -i;
                component.TryBeginBusy = (o, type, arg3) =>
                {
                    methodCallCount++;
                    o.ShouldEqual(componentOwner);
                    type.ShouldEqual(typeof(BusyManager));
                    arg3.ShouldEqual(DefaultMetadata);
                    if (i1 == componentCount - 1)
                        return busyToken;
                    return null;
                };
                componentOwner.AddComponent(component);
            }

            componentOwner.BeginBusy(componentOwner, DefaultMetadata).ShouldEqual(busyToken);
            methodCallCount.ShouldEqual(componentCount);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(100)]
        public void TryGetTokenShouldBeHandledByComponents(int componentCount)
        {
            var componentOwner = GetComponentOwner();
            var busyToken = new TestBusyToken();
            var methodCallCount = 0;
            Func<BusyManager, IBusyToken, IReadOnlyMetadataContext?, bool> filter = (manager, token, arg3) => true;
            for (var i = 0; i < componentCount; i++)
            {
                var component = new TestBusyManagerComponent();
                var i1 = i;
                component.Priority = -i;
                component.TryGetToken = (del, o, arg3, arg4, arg5) =>
                {
                    methodCallCount++;
                    o.ShouldEqual(componentOwner);
                    arg3.ShouldEqual(typeof(BusyManager));
                    arg4.ShouldEqual(filter);
                    arg5.ShouldEqual(DefaultMetadata);
                    if (i1 == componentCount - 1)
                        return busyToken;
                    return null;
                };
                componentOwner.AddComponent(component);
            }

            componentOwner.TryGetToken(componentOwner, filter, DefaultMetadata).ShouldEqual(busyToken);
            methodCallCount.ShouldEqual(componentCount);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(100)]
        public void GetTokensShouldBeHandledByComponents(int componentCount)
        {
            var componentOwner = GetComponentOwner();
            var methodCallCount = 0;
            for (var i = 0; i < componentCount; i++)
            {
                var component = new TestBusyManagerComponent();
                component.TryGetTokens = context =>
                {
                    ++methodCallCount;
                    context.ShouldEqual(DefaultMetadata);
                    return new[] { new TestBusyToken(), new TestBusyToken() };
                };
                componentOwner.AddComponent(component);
            }

            new HashSet<IBusyToken>(componentOwner.GetTokens(DefaultMetadata)).Count.ShouldEqual(componentCount * 2);
            methodCallCount.ShouldEqual(componentCount);
        }

        protected override BusyManager GetComponentOwner(IComponentCollectionProvider? collectionProvider = null)
        {
            return new BusyManager(collectionProvider);
        }

        #endregion
    }
}