using System;
using System.Collections.Generic;
using MugenMvvm.Busy;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Busy;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Tests.Busy;
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
            var busyToken = new TestBusyToken();
            var methodCallCount = 0;
            for (var i = 0; i < componentCount; i++)
            {
                var i1 = i;
                BusyManager.AddComponent(new TestBusyManagerComponent
                {
                    Priority = -i,
                    TryBeginBusy = (b, o, m) =>
                    {
                        b.ShouldEqual(BusyManager);
                        o.ShouldEqual(BusyManager);
                        m.ShouldEqual(DefaultMetadata);
                        methodCallCount++;
                        if (i1 == componentCount - 1)
                            return busyToken;
                        return null;
                    }
                });
            }

            BusyManager.BeginBusy(BusyManager, DefaultMetadata).ShouldEqual(busyToken);
            methodCallCount.ShouldEqual(componentCount);
        }

        [Fact]
        public void BeginBusyShouldThrowNoComponents() => ShouldThrow<InvalidOperationException>(() => BusyManager.BeginBusy(this));

        [Theory]
        [InlineData(1)]
        [InlineData(100)]
        public void GetTokensShouldBeHandledByComponents(int componentCount)
        {
            var methodCallCount = 0;
            for (var i = 0; i < componentCount; i++)
            {
                BusyManager.AddComponent(new TestBusyManagerComponent
                {
                    TryGetTokens = (o, context) =>
                    {
                        ++methodCallCount;
                        o.ShouldEqual(BusyManager);
                        context.ShouldEqual(DefaultMetadata);
                        return new[] { new TestBusyToken(), new TestBusyToken() };
                    }
                });
            }

            new HashSet<IBusyToken>(BusyManager.GetTokens(DefaultMetadata)).Count.ShouldEqual(componentCount * 2);
            methodCallCount.ShouldEqual(componentCount);
        }

        [Fact]
        public void GetTokensShouldReturnEmptyNoComponents() => BusyManager.GetTokens().ShouldBeEmpty();

        [Fact]
        public void ShouldValidateInputArgs() => ShouldThrow<ArgumentNullException>(() => BusyManager.TryGetToken(this, null!));

        [Theory]
        [InlineData(1)]
        [InlineData(100)]
        public void TryGetTokenShouldBeHandledByComponents(int componentCount)
        {
            var busyToken = new TestBusyToken();
            var methodCallCount = 0;
            Func<IBusyManager, IBusyToken, IReadOnlyMetadataContext?, bool> filter = (manager, token, arg3) => true;
            for (var i = 0; i < componentCount; i++)
            {
                var i1 = i;
                BusyManager.AddComponent(new TestBusyManagerComponent
                {
                    Priority = -i,
                    TryGetToken = (b, del, o, arg5) =>
                    {
                        methodCallCount++;
                        b.ShouldEqual(BusyManager);
                        o.ShouldEqual(BusyManager);
                        arg5.ShouldEqual(DefaultMetadata);
                        if (i1 == componentCount - 1)
                            return busyToken;
                        return null;
                    }
                });
            }

            BusyManager.TryGetToken(BusyManager, filter, DefaultMetadata).ShouldEqual(busyToken);
            methodCallCount.ShouldEqual(componentCount);
        }

        [Fact]
        public void TryGetTokenShouldReturnNullNoComponents() => BusyManager.TryGetToken(this, (_, _, _) => true).ShouldBeNull();

        protected override IBusyManager GetBusyManager() => GetComponentOwner(ComponentCollectionManager);

        protected override BusyManager GetComponentOwner(IComponentCollectionManager? componentCollectionManager = null) => new(componentCollectionManager);
    }
}