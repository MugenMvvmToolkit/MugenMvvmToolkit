using System;
using System.Collections.Generic;
using MugenMvvm.Busy;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Busy;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.UnitTests.Busy.Internal;
using MugenMvvm.UnitTests.Components;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Busy
{
    public class BusyManagerTest : ComponentOwnerTestBase<BusyManager>
    {
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
                var i1 = i;
                var component = new TestBusyManagerComponent(componentOwner)
                {
                    Priority = -i,
                    TryBeginBusy = (o, arg3) =>
                    {
                        methodCallCount++;
                        o.ShouldEqual(componentOwner);
                        arg3.ShouldEqual(DefaultMetadata);
                        if (i1 == componentCount - 1)
                            return busyToken;
                        return null;
                    }
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
                var i1 = i;
                var component = new TestBusyManagerComponent(componentOwner)
                {
                    Priority = -i,
                    TryGetToken = (del, o, arg5) =>
                    {
                        methodCallCount++;
                        o.ShouldEqual(componentOwner);
                        arg5.ShouldEqual(DefaultMetadata);
                        if (i1 == componentCount - 1)
                            return busyToken;
                        return null;
                    }
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
                var component = new TestBusyManagerComponent(componentOwner)
                {
                    TryGetTokens = context =>
                    {
                        ++methodCallCount;
                        context.ShouldEqual(DefaultMetadata);
                        return new[] {new TestBusyToken(), new TestBusyToken()};
                    }
                };
                componentOwner.AddComponent(component);
            }

            new HashSet<IBusyToken>(componentOwner.GetTokens(DefaultMetadata).AsList()).Count.ShouldEqual(componentCount * 2);
            methodCallCount.ShouldEqual(componentCount);
        }

        protected override BusyManager GetComponentOwner(IComponentCollectionManager? collectionProvider = null) => new(collectionProvider);

        [Fact]
        public void BeginBusyShouldThrowNoComponents()
        {
            var componentOwner = GetComponentOwner();
            ShouldThrow<InvalidOperationException>(() => componentOwner.BeginBusy(componentOwner));
        }

        [Fact]
        public void GetTokensShouldReturnEmptyNoComponents()
        {
            var componentOwner = GetComponentOwner();
            componentOwner.GetTokens().AsList().ShouldBeEmpty();
        }

        [Fact]
        public void ShouldValidateInputArgs()
        {
            var componentOwner = GetComponentOwner();
            ShouldThrow<ArgumentNullException>(() => componentOwner.TryGetToken(this, null!));
        }

        [Fact]
        public void TryGetTokenShouldReturnNullNoComponents()
        {
            var componentOwner = GetComponentOwner();
            componentOwner.TryGetToken(this, (manager, token, arg3) => true).ShouldBeNull();
        }
    }
}