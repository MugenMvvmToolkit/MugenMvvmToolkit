using System;
using System.Collections.Generic;
using MugenMvvm.Busy;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Busy;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.UnitTest.Busy.Internal;
using MugenMvvm.UnitTest.Components;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Busy
{
    public class BusyManagerTest : ComponentOwnerTestBase<BusyManager>
    {
        #region Methods

        [Fact]
        public void ShouldValidateInputArgs()
        {
            var componentOwner = GetComponentOwner();
            ShouldThrow<ArgumentNullException>(() => componentOwner.TryGetToken(null!));
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
            componentOwner.TryGetToken((manager, token, arg3) => true).ShouldBeNull();
        }

        [Fact]
        public void GetTokensShouldReturnEmptyNoComponents()
        {
            var componentOwner = GetComponentOwner();
            componentOwner.GetTokens().AsList().ShouldBeEmpty();
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
                component.TryBeginBusy = (m, o, arg3) =>
                {
                    methodCallCount++;
                    m.ShouldEqual(componentOwner);
                    o.ShouldEqual(componentOwner);
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
            Func<object?, IBusyToken, IReadOnlyMetadataContext?, bool> filter = (manager, token, arg3) => true;
            for (var i = 0; i < componentCount; i++)
            {
                var component = new TestBusyManagerComponent();
                var i1 = i;
                component.Priority = -i;
                component.TryGetToken = (m, del, o, arg5) =>
                {
                    methodCallCount++;
                    m.ShouldEqual(componentOwner);
                    o.ShouldEqual(componentOwner);
                    arg5.ShouldEqual(DefaultMetadata);
                    if (i1 == componentCount - 1)
                        return busyToken;
                    return null;
                };
                componentOwner.AddComponent(component);
            }

            componentOwner.TryGetToken(filter, componentOwner, DefaultMetadata).ShouldEqual(busyToken);
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
                component.TryGetTokens = (m, context) =>
                {
                    ++methodCallCount;
                    m.ShouldEqual(componentOwner);
                    context.ShouldEqual(DefaultMetadata);
                    return new[] {new TestBusyToken(), new TestBusyToken()};
                };
                componentOwner.AddComponent(component);
            }

            new HashSet<IBusyToken>(componentOwner.GetTokens(DefaultMetadata).AsList()).Count.ShouldEqual(componentCount * 2);
            methodCallCount.ShouldEqual(componentCount);
        }

        protected override BusyManager GetComponentOwner(IComponentCollectionManager? collectionProvider = null)
        {
            return new BusyManager(collectionProvider);
        }

        #endregion
    }
}