using System.Collections.Generic;
using System.Threading.Tasks;
using MugenMvvm.Busy;
using MugenMvvm.Busy.Components;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Busy;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.UnitTest.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Busy
{
    public class BusyManagerComponentTest : UnitTestBase
    {
        #region Fields

        private const int DefaultDelay = 150;
        private readonly BusyManager _busyManager;
        private readonly TestBusyManagerListener _listener;

        #endregion

        #region Constructors

        public BusyManagerComponentTest()
        {
            _busyManager = new BusyManager();
            _listener = new TestBusyManagerListener();
            _busyManager.AddComponent(_listener);
        }

        #endregion

        #region Methods

        [Fact]
        public void ShouldNotifyListenersOnAttach()
        {
            var count = 0;
            var busyManagerComponent = new BusyManagerComponent();
            _listener.OnBusyChanged = (manager, metadata) =>
            {
                manager.ShouldEqual(_busyManager);
                ++count;
            };

            count.ShouldEqual(0);
            using var subscriber = TestComponentSubscriber.Subscribe(_busyManager, busyManagerComponent);
            count.ShouldEqual(1);
        }

        [Fact]
        public void ShouldNotifyListenersOnDetach()
        {
            var count = 0;
            var busyManagerComponent = new BusyManagerComponent();
            var subscriber = TestComponentSubscriber.Subscribe(_busyManager, busyManagerComponent);

            _listener.OnBusyChanged = (manager, metadata) =>
            {
                manager.ShouldEqual(_busyManager);
                ++count;
            };
            count.ShouldEqual(0);
            subscriber.Dispose();
            count.ShouldEqual(1);
        }

        [Fact]
        public void TryBeginBusyShouldNotifyListeners()
        {
            var count = 0;
            var busyManagerComponent = new BusyManagerComponent();
            using var subscriber = TestComponentSubscriber.Subscribe(_busyManager, busyManagerComponent);

            _listener.OnBusyChanged = (manager, metadata) =>
            {
                manager.ShouldEqual(_busyManager);
                ++count;
            };
            count.ShouldEqual(0);
            busyManagerComponent.TryBeginBusy(busyManagerComponent, null);
            count.ShouldEqual(1);
        }

        [Fact]
        public void SuspendShouldNotifyListeners()
        {
            var count = 0;
            var busyManagerComponent = new BusyManagerComponent();
            busyManagerComponent.TryBeginBusy(busyManagerComponent, null);
            using var subscriber = TestComponentSubscriber.Subscribe(_busyManager, busyManagerComponent);

            _listener.OnBusyChanged = (manager, metadata) =>
            {
                manager.ShouldEqual(_busyManager);
                ++count;
            };
            count.ShouldEqual(0);
            var actionToken = busyManagerComponent.Suspend();
            count.ShouldEqual(1);
            actionToken.Dispose();
            count.ShouldEqual(2);
        }

        [Fact]
        public void SuspendTokenShouldNotifyListeners()
        {
            var count = 0;
            var busyManagerComponent = new BusyManagerComponent();
            var busyToken = busyManagerComponent.TryBeginBusy(busyManagerComponent, null)!;
            using var subscriber = TestComponentSubscriber.Subscribe(_busyManager, busyManagerComponent);

            _listener.OnBusyChanged = (manager, metadata) =>
            {
                manager.ShouldEqual(_busyManager);
                ++count;
            };
            count.ShouldEqual(0);
            var actionToken = busyToken.Suspend();
            count.ShouldEqual(1);
            actionToken.Dispose();
            count.ShouldEqual(2);
        }

        [Fact]
        public void DisposeTokenShouldNotifyListeners()
        {
            var count = 0;
            var busyManagerComponent = new BusyManagerComponent();
            var busyToken = busyManagerComponent.TryBeginBusy(busyManagerComponent, null)!;
            using var subscriber = TestComponentSubscriber.Subscribe(_busyManager, busyManagerComponent);

            _listener.OnBusyChanged = (manager, metadata) =>
            {
                manager.ShouldEqual(_busyManager);
                ++count;
            };
            count.ShouldEqual(0);
            busyToken.Dispose();
            count.ShouldEqual(1);
        }

        [Theory]
        [InlineData(null, 0, true)]
        [InlineData(null, 0, false)]
        [InlineData(null, DefaultDelay, true)]
        [InlineData(null, DefaultDelay, false)]
        [InlineData(1, 0, true)]
        [InlineData(1, 0, false)]
        [InlineData(1, DefaultDelay, true)]
        [InlineData(1, DefaultDelay, false)]
        public void ShouldTryBeginBusyWithMessage(object? message, int delay, bool includeMetadata)
        {
            var count = 0;
            var busyManagerComponent = new BusyManagerComponent();
            using var subscriber = TestComponentSubscriber.Subscribe(_busyManager, busyManagerComponent);
            IBusyToken? busyToken = null;
            var meta = includeMetadata ? DefaultMetadata : null;

            _listener.OnBeginBusy = (manager, token, metadata) =>
            {
                ++count;
                metadata.ShouldEqual(meta);
                manager.ShouldEqual(_busyManager);
                busyToken = token;
            };

            var beginBusy = delay == 0
                ? busyManagerComponent.TryBeginBusy(message, meta)!
                : busyManagerComponent.TryBeginBusy(new BeginBusyRequest(message, delay), meta)!;
            beginBusy.Message.ShouldEqual(message);

            if (delay == 0)
            {
                beginBusy.ShouldEqual(busyToken);
                count.ShouldEqual(1);
                return;
            }

            count.ShouldEqual(0);
            Task.Delay(delay + 100).Wait();
            beginBusy.ShouldEqual(busyToken);
            count.ShouldEqual(1);
        }

        [Theory]
        [InlineData(null, 0, true)]
        [InlineData(null, 0, false)]
        [InlineData(null, DefaultDelay, true)]
        [InlineData(null, DefaultDelay, false)]
        [InlineData(1, 0, true)]
        [InlineData(1, 0, false)]
        [InlineData(1, DefaultDelay, true)]
        [InlineData(1, DefaultDelay, false)]
        public void ShouldTryBeginBusyWithParentToken(object? message, int delay, bool includeMetadata)
        {
            var count = 0;
            var busyManagerComponent = new BusyManagerComponent();
            var parentToken = busyManagerComponent.TryBeginBusy(message, null);
            using var subscriber = TestComponentSubscriber.Subscribe(_busyManager, busyManagerComponent);
            IBusyToken? busyToken = null;
            var meta = includeMetadata ? DefaultMetadata : null;

            _listener.OnBeginBusy = (manager, token, metadata) =>
            {
                ++count;
                metadata.ShouldEqual(meta);
                manager.ShouldEqual(_busyManager);
                busyToken = token;
            };

            var beginBusy = delay == 0
                ? busyManagerComponent.TryBeginBusy(parentToken, meta)!
                : busyManagerComponent.TryBeginBusy(new BeginBusyRequest(parentToken, delay), meta)!;
            beginBusy.Message.ShouldEqual(message);

            if (delay == 0)
            {
                beginBusy.ShouldEqual(busyToken);
                count.ShouldEqual(1);
                return;
            }
            count.ShouldEqual(0);
            Task.Delay(delay + 100).Wait();
            beginBusy.ShouldEqual(busyToken);
            count.ShouldEqual(1);
        }

        [Theory]
        [InlineData(null, DefaultDelay)]
        [InlineData(1, DefaultDelay)]
        public void TryBeginBusyShouldIgnoreListenersDisposeBeforeDelay(object? message, int delay)
        {
            var count = 0;
            var busyManagerComponent = new BusyManagerComponent();
            using var subscriber = TestComponentSubscriber.Subscribe(_busyManager, busyManagerComponent);
            _listener.OnBeginBusy = (manager, token, metadata) =>
            {
                ++count;
            };

            var beginBusy = busyManagerComponent.TryBeginBusy(new BeginBusyRequest(message, delay), null)!;
            beginBusy.Dispose();

            count.ShouldEqual(0);
            Task.Delay(delay + 100).Wait();
            count.ShouldEqual(0);
        }

        [Theory]
        [InlineData(null, DefaultDelay)]
        [InlineData(1, DefaultDelay)]
        public void TryBeginBusyShouldIgnoreListenersDisposeBeforeDelayParentToken(object? message, int delay)
        {
            var count = 0;
            var busyManagerComponent = new BusyManagerComponent();
            var parentToken = busyManagerComponent.TryBeginBusy(message, null);
            using var subscriber = TestComponentSubscriber.Subscribe(_busyManager, busyManagerComponent);
            _listener.OnBeginBusy = (manager, token, metadata) =>
            {
                ++count;
            };

            var beginBusy = busyManagerComponent.TryBeginBusy(new BeginBusyRequest(parentToken, delay), null)!;
            beginBusy.Dispose();

            count.ShouldEqual(0);
            Task.Delay(delay + 100).Wait();
            count.ShouldEqual(0);
        }

        [Fact]
        public void TokenShouldUpdateCompletedState()
        {
            var busyManagerComponent = new BusyManagerComponent();
            var token = busyManagerComponent.TryBeginBusy("Test", null)!;
            var callback = new TestBusyTokenCallback();
            int count = 0;
            callback.OnCompleted += busyToken =>
            {
                ++count;
                busyToken.ShouldEqual(token);
            };

            token.RegisterCallback(callback);
            count.ShouldEqual(0);
            token.IsCompleted.ShouldBeFalse();

            token.Dispose();
            count.ShouldEqual(1);
            token.IsCompleted.ShouldBeTrue();
        }

        [Fact]
        public void TokenShouldUpdateSuspendedState()
        {
            var busyManagerComponent = new BusyManagerComponent();
            var token = busyManagerComponent.TryBeginBusy("Test", null)!;
            var callback = new TestBusyTokenCallback();
            int count = 0;
            bool suspendValue = false;
            callback.OnSuspendChanged += suspended =>
            {
                ++count;
                suspended.ShouldEqual(suspendValue);
            };

            token.RegisterCallback(callback);
            count.ShouldEqual(0);
            token.IsSuspended.ShouldBeFalse();

            suspendValue = true;
            var actionToken = token.Suspend();
            token.IsSuspended.ShouldBeTrue();
            count.ShouldEqual(1);

            suspendValue = false;
            actionToken.Dispose();
            count.ShouldEqual(2);
            token.IsSuspended.ShouldBeFalse();
        }

        [Fact]
        public void TokenShouldUpdateCompletedStateParentToken()
        {
            var busyManagerComponent = new BusyManagerComponent();
            var parentToken = busyManagerComponent.TryBeginBusy("Test", null)!;
            var token = busyManagerComponent.TryBeginBusy(parentToken, null)!;
            var callback = new TestBusyTokenCallback();
            int count = 0;
            callback.OnCompleted += busyToken =>
            {
                ++count;
                busyToken.ShouldEqual(token);
            };

            token.RegisterCallback(callback);
            count.ShouldEqual(0);
            token.IsCompleted.ShouldBeFalse();
            parentToken.IsCompleted.ShouldBeFalse();

            token.Dispose();
            count.ShouldEqual(1);
            parentToken.IsCompleted.ShouldBeFalse();
            token.IsCompleted.ShouldBeTrue();
        }

        [Fact]
        public void TokenShouldUpdateSuspendedStateParentToken()
        {
            var busyManagerComponent = new BusyManagerComponent();
            var parentToken = busyManagerComponent.TryBeginBusy("Test", null)!;
            var token = busyManagerComponent.TryBeginBusy(parentToken, null)!;
            var callback = new TestBusyTokenCallback();
            int count = 0;
            bool suspendValue = false;
            callback.OnSuspendChanged += suspended =>
            {
                ++count;
                suspended.ShouldEqual(suspendValue);
            };

            token.RegisterCallback(callback);
            count.ShouldEqual(0);
            token.IsSuspended.ShouldBeFalse();
            parentToken.IsSuspended.ShouldBeFalse();

            suspendValue = true;
            var actionToken = token.Suspend();
            parentToken.IsSuspended.ShouldBeFalse();
            token.IsSuspended.ShouldBeTrue();
            count.ShouldEqual(1);

            suspendValue = false;
            actionToken.Dispose();
            count.ShouldEqual(2);
            parentToken.IsSuspended.ShouldBeFalse();
            token.IsSuspended.ShouldBeFalse();
        }

        [Fact]
        public void TokenShouldUpdateCompletedStateParentTokenDisposed()
        {
            var busyManagerComponent = new BusyManagerComponent();
            var parentToken = busyManagerComponent.TryBeginBusy("Test", null)!;
            var token = busyManagerComponent.TryBeginBusy(parentToken, null)!;
            var callback = new TestBusyTokenCallback();
            int count = 0;
            callback.OnCompleted += busyToken =>
            {
                ++count;
                busyToken.ShouldEqual(token);
            };

            token.RegisterCallback(callback);
            count.ShouldEqual(0);
            token.IsCompleted.ShouldBeFalse();
            parentToken.IsCompleted.ShouldBeFalse();

            parentToken.Dispose();
            count.ShouldEqual(1);
            parentToken.IsCompleted.ShouldBeTrue();
            token.IsCompleted.ShouldBeTrue();
        }

        [Fact]
        public void TokenShouldUpdateSuspendedStateParentTokenSuspended()
        {
            var busyManagerComponent = new BusyManagerComponent();
            var parentToken = busyManagerComponent.TryBeginBusy("Test", null)!;
            var token = busyManagerComponent.TryBeginBusy(parentToken, null)!;
            var callback = new TestBusyTokenCallback();
            int count = 0;
            bool suspendValue = false;
            callback.OnSuspendChanged += suspended =>
            {
                ++count;
                suspended.ShouldEqual(suspendValue);
            };

            token.RegisterCallback(callback);
            count.ShouldEqual(0);
            token.IsSuspended.ShouldBeFalse();
            parentToken.IsSuspended.ShouldBeFalse();

            suspendValue = true;
            var actionToken = parentToken.Suspend();
            parentToken.IsSuspended.ShouldBeTrue();
            token.IsSuspended.ShouldBeTrue();
            count.ShouldEqual(1);

            suspendValue = false;
            actionToken.Dispose();
            count.ShouldEqual(2);
            parentToken.IsSuspended.ShouldBeFalse();
            token.IsSuspended.ShouldBeFalse();
        }

        [Theory]
        [InlineData(1, false)]
        [InlineData(10, false)]
        [InlineData(1, true)]
        [InlineData(10, true)]
        public void TokenShouldRegisterCallbacks(int callbackCount, bool unsubscribe)
        {
            var busyManagerComponent = new BusyManagerComponent();
            var token = busyManagerComponent.TryBeginBusy("Test", null)!;
            int completedCount = 0;
            int suspendedCount = 0;
            var callbacks = new ActionToken[callbackCount];

            for (int i = 0; i < callbackCount; i++)
            {
                var callback = new TestBusyTokenCallback();
                callback.OnCompleted = busyToken => { ++completedCount; };
                callback.OnSuspendChanged = b => { ++suspendedCount; };
                callbacks[i] = token.RegisterCallback(callback);
            }

            if (unsubscribe)
            {
                callbackCount = 0;
                for (int i = 0; i < callbacks.Length; i++)
                    callbacks[i].Dispose();
            }
            suspendedCount.ShouldEqual(0);
            var actionToken = token.Suspend();
            suspendedCount.ShouldEqual(callbackCount);
            actionToken.Dispose();
            suspendedCount.ShouldEqual(callbackCount * 2);

            completedCount.ShouldEqual(0);
            token.Dispose();
            completedCount.ShouldEqual(callbackCount);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void TokenShouldInvokeCallbackCompleted(int callbackCount)
        {
            var busyManagerComponent = new BusyManagerComponent();
            var token = busyManagerComponent.TryBeginBusy("Test", null)!;
            token.Dispose();
            int completedCount = 0;

            for (int i = 0; i < callbackCount; i++)
            {
                var callback = new TestBusyTokenCallback();
                callback.OnCompleted = busyToken => { ++completedCount; };
                token.RegisterCallback(callback);
            }

            completedCount.ShouldEqual(callbackCount);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void TokenShouldInvokeCallbackSuspended(int callbackCount)
        {
            var busyManagerComponent = new BusyManagerComponent();
            var token = busyManagerComponent.TryBeginBusy("Test", null)!;
            token.Suspend();
            int suspendedCount = 0;

            for (int i = 0; i < callbackCount; i++)
            {
                var callback = new TestBusyTokenCallback();
                callback.OnSuspendChanged = b => { ++suspendedCount; };
                token.RegisterCallback(callback);
            }
            suspendedCount.ShouldEqual(callbackCount);
        }

        [Fact]
        public void SuspendShouldSuspendAllTokens()
        {
            var busyManagerComponent = new BusyManagerComponent();
            var token1 = busyManagerComponent.TryBeginBusy("Test", null)!;
            var token2 = busyManagerComponent.TryBeginBusy("Test", null)!;

            token1.IsSuspended.ShouldBeFalse();
            token2.IsSuspended.ShouldBeFalse();

            var actionToken = busyManagerComponent.Suspend();
            token1.IsSuspended.ShouldBeTrue();
            token2.IsSuspended.ShouldBeTrue();

            actionToken.Dispose();
            token1.IsSuspended.ShouldBeFalse();
            token2.IsSuspended.ShouldBeFalse();
        }

        [Fact]
        public void SuspendShouldCombineWithTokenSuspend()
        {
            var busyManagerComponent = new BusyManagerComponent();
            var token1 = busyManagerComponent.TryBeginBusy("Test", null)!;
            var token2 = busyManagerComponent.TryBeginBusy("Test", null)!;

            token1.IsSuspended.ShouldBeFalse();
            token2.IsSuspended.ShouldBeFalse();

            var tokenSuspend1 = token1.Suspend();
            var tokenSuspend2 = token2.Suspend();
            token1.IsSuspended.ShouldBeTrue();
            token2.IsSuspended.ShouldBeTrue();

            var actionToken = busyManagerComponent.Suspend();
            token1.IsSuspended.ShouldBeTrue();
            token2.IsSuspended.ShouldBeTrue();

            tokenSuspend1.Dispose();
            actionToken.Dispose();
            token1.IsSuspended.ShouldBeFalse();
            token2.IsSuspended.ShouldBeTrue();

            tokenSuspend2.Dispose();
            token2.IsSuspended.ShouldBeFalse();
        }

        [Fact]
        public void TryGetTokensShouldReturnAllTokens()
        {
            var busyManagerComponent = new BusyManagerComponent();
            var token1 = busyManagerComponent.TryBeginBusy("Test1", null)!;
            var token2 = busyManagerComponent.TryBeginBusy("Test2", null)!;

            var tokens = busyManagerComponent.TryGetTokens(DefaultMetadata)!;
            tokens.Count.ShouldEqual(2);
            tokens.ShouldContain(token1);
            tokens.ShouldContain(token2);

            token1.Dispose();
            tokens = busyManagerComponent.TryGetTokens(DefaultMetadata)!;
            tokens.Count.ShouldEqual(1);
            tokens.ShouldContain(token2);

            token2.Dispose();
            tokens = busyManagerComponent.TryGetTokens(DefaultMetadata)!;
            (tokens == null || tokens.Count == 0).ShouldBeTrue();
        }

        [Fact]
        public void TryGetTokenShouldReturnAllTokens()
        {
            var busyManagerComponent = new BusyManagerComponent();
            var token1 = busyManagerComponent.TryBeginBusy("Test1", null)!;
            var token2 = busyManagerComponent.TryBeginBusy("Test2", null)!;
            var tokens = new List<IBusyToken>();

            busyManagerComponent.TryGetToken(busyManagerComponent, (in BusyManagerComponent component, IBusyToken token, IReadOnlyMetadataContext? arg3) =>
            {
                busyManagerComponent.ShouldEqual(component);
                tokens.Add(token);
                arg3.ShouldEqual(DefaultMetadata);
                return false;
            }, DefaultMetadata);
            tokens.Count.ShouldEqual(2);
            tokens.ShouldContain(token1);
            tokens.ShouldContain(token2);
        }

        [Fact]
        public void TryGetTokenShouldReturnCorrectToken()
        {
            var busyManagerComponent = new BusyManagerComponent();
            var token1 = busyManagerComponent.TryBeginBusy("Test1", null)!;
            var token2 = busyManagerComponent.TryBeginBusy("Test2", null)!;
            var tokens = new List<IBusyToken>();

            busyManagerComponent
                .TryGetToken(busyManagerComponent, (in BusyManagerComponent component, IBusyToken token, IReadOnlyMetadataContext? arg3) => ReferenceEquals(token, token1), DefaultMetadata)
                .ShouldEqual(token1);
            busyManagerComponent
                .TryGetToken(busyManagerComponent, (in BusyManagerComponent component, IBusyToken token, IReadOnlyMetadataContext? arg3) => ReferenceEquals(token, token2), DefaultMetadata)
                .ShouldEqual(token2);
            busyManagerComponent
                .TryGetToken(busyManagerComponent, (in BusyManagerComponent component, IBusyToken token, IReadOnlyMetadataContext? arg3) => false, DefaultMetadata)
                .ShouldBeNull();
        }

        #endregion
    }
}