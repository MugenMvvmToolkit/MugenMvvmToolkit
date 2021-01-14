using System.Collections.Generic;
using MugenMvvm.Busy;
using MugenMvvm.Busy.Components;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Busy;
using MugenMvvm.Internal;
using MugenMvvm.UnitTests.Busy.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Busy.Components
{
    public class BusyManagerComponentTest : UnitTestBase
    {
        private const int DefaultDelay = 150;

        private readonly BusyManager _busyManager;
        private readonly TestBusyManagerListener _listener;

        public BusyManagerComponentTest()
        {
            _busyManager = new BusyManager();
            _listener = new TestBusyManagerListener(_busyManager);
            _busyManager.AddComponent(_listener);
        }

        [Fact]
        public void DisposeTokenShouldNotifyListeners()
        {
            var count = 0;
            var busyManagerComponent = new BusyManagerComponent();
            using var t = _busyManager.AddComponent(busyManagerComponent);
            var busyToken = busyManagerComponent.TryBeginBusy(_busyManager, busyManagerComponent, null)!;

            _listener.OnBusyChanged = metadata => { ++count; };
            count.ShouldEqual(0);
            busyToken.Dispose();
            count.ShouldEqual(1);
        }

        [Fact]
        public void OnAttachShouldNotifyListeners()
        {
            var count = 0;
            var busyManagerComponent = new BusyManagerComponent();
            _listener.OnBusyChanged = metadata => { ++count; };

            count.ShouldEqual(0);
            using var t = _busyManager.AddComponent(busyManagerComponent);
            count.ShouldEqual(1);
        }

        [Fact]
        public void OnDetachShouldNotifyListeners()
        {
            var count = 0;
            var busyManagerComponent = new BusyManagerComponent();
            using var t = _busyManager.AddComponent(busyManagerComponent);

            _listener.OnBusyChanged = metadata => { ++count; };
            count.ShouldEqual(0);
            t.Dispose();
            count.ShouldEqual(1);
        }

        [Fact]
        public void SuspendTokenShouldNotifyListeners()
        {
            var count = 0;
            var busyManagerComponent = new BusyManagerComponent();
            using var t = _busyManager.AddComponent(busyManagerComponent);
            var busyToken = busyManagerComponent.TryBeginBusy(_busyManager, busyManagerComponent, null)!;

            _listener.OnBusyChanged = metadata => { ++count; };
            count.ShouldEqual(0);
            var actionToken = busyToken.Suspend(this, DefaultMetadata);
            count.ShouldEqual(1);
            actionToken.Dispose();
            count.ShouldEqual(2);
        }

        [Fact]
        public void TokenShouldUpdateCompletedState()
        {
            var busyManagerComponent = new BusyManagerComponent();
            using var t = _busyManager.AddComponent(busyManagerComponent);
            var token = busyManagerComponent.TryBeginBusy(_busyManager, "Test", null)!;
            var callback = new TestBusyTokenCallback();
            var count = 0;
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
        public void TokenShouldUpdateCompletedStateParentToken()
        {
            var busyManagerComponent = new BusyManagerComponent();
            using var t = _busyManager.AddComponent(busyManagerComponent);
            var parentToken = busyManagerComponent.TryBeginBusy(_busyManager, "Test", null)!;
            var token = busyManagerComponent.TryBeginBusy(_busyManager, parentToken, null)!;
            var callback = new TestBusyTokenCallback();
            var count = 0;
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
        public void TokenShouldUpdateCompletedStateParentTokenDisposed()
        {
            var busyManagerComponent = new BusyManagerComponent();
            using var t = _busyManager.AddComponent(busyManagerComponent);
            var parentToken = busyManagerComponent.TryBeginBusy(_busyManager, "Test", null)!;
            var token = busyManagerComponent.TryBeginBusy(_busyManager, parentToken, null)!;
            var callback = new TestBusyTokenCallback();
            var count = 0;
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
        public void TokenShouldUpdateSuspendedState()
        {
            var busyManagerComponent = new BusyManagerComponent();
            using var t = _busyManager.AddComponent(busyManagerComponent);
            var token = busyManagerComponent.TryBeginBusy(_busyManager, "Test", null)!;
            var callback = new TestBusyTokenCallback();
            var count = 0;
            var suspendValue = false;
            callback.OnSuspendChanged += suspended =>
            {
                ++count;
                suspended.ShouldEqual(suspendValue);
            };

            token.RegisterCallback(callback);
            count.ShouldEqual(0);
            token.IsSuspended.ShouldBeFalse();

            suspendValue = true;
            var actionToken = token.Suspend(this, DefaultMetadata);
            token.IsSuspended.ShouldBeTrue();
            count.ShouldEqual(1);

            suspendValue = false;
            actionToken.Dispose();
            count.ShouldEqual(2);
            token.IsSuspended.ShouldBeFalse();
        }

        [Fact]
        public void TokenShouldUpdateSuspendedStateParentToken()
        {
            var busyManagerComponent = new BusyManagerComponent();
            using var t = _busyManager.AddComponent(busyManagerComponent);
            var parentToken = busyManagerComponent.TryBeginBusy(_busyManager, "Test", null)!;
            var token = busyManagerComponent.TryBeginBusy(_busyManager, parentToken, null)!;
            var callback = new TestBusyTokenCallback();
            var count = 0;
            var suspendValue = false;
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
            var actionToken = token.Suspend(this, DefaultMetadata);
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
        public void TokenShouldUpdateSuspendedStateParentTokenSuspended()
        {
            var busyManagerComponent = new BusyManagerComponent();
            using var t = _busyManager.AddComponent(busyManagerComponent);
            var parentToken = busyManagerComponent.TryBeginBusy(_busyManager, "Test", null)!;
            var token = busyManagerComponent.TryBeginBusy(_busyManager, parentToken, null)!;
            var callback = new TestBusyTokenCallback();
            var count = 0;
            var suspendValue = false;
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
            var actionToken = parentToken.Suspend(this, DefaultMetadata);
            parentToken.IsSuspended.ShouldBeTrue();
            token.IsSuspended.ShouldBeTrue();
            count.ShouldEqual(1);

            suspendValue = false;
            actionToken.Dispose();
            count.ShouldEqual(2);
            parentToken.IsSuspended.ShouldBeFalse();
            token.IsSuspended.ShouldBeFalse();
        }

        [Fact]
        public void TryBeginBusyShouldNotifyListeners()
        {
            var count = 0;
            var busyManagerComponent = new BusyManagerComponent();
            using var t = _busyManager.AddComponent(busyManagerComponent);

            _listener.OnBusyChanged = metadata => { ++count; };
            count.ShouldEqual(0);
            busyManagerComponent.TryBeginBusy(_busyManager, busyManagerComponent, null);
            count.ShouldEqual(1);
        }

        [Fact]
        public void TryGetTokenShouldReturnAllTokens()
        {
            var busyManagerComponent = new BusyManagerComponent();
            using var t = _busyManager.AddComponent(busyManagerComponent);
            var token1 = busyManagerComponent.TryBeginBusy(_busyManager, "Test1", null)!;
            var token2 = busyManagerComponent.TryBeginBusy(_busyManager, "Test2", null)!;
            var tokens = new List<IBusyToken>();

            busyManagerComponent.TryGetToken(_busyManager, (component, token, arg3) =>
            {
                busyManagerComponent.ShouldEqual(component);
                tokens.Add(token);
                arg3.ShouldEqual(DefaultMetadata);
                return false;
            }, busyManagerComponent, DefaultMetadata);
            tokens.Count.ShouldEqual(2);
            tokens.ShouldContain(token1);
            tokens.ShouldContain(token2);
        }

        [Fact]
        public void TryGetTokenShouldReturnCorrectToken()
        {
            var busyManagerComponent = new BusyManagerComponent();
            using var t = _busyManager.AddComponent(busyManagerComponent);
            var token1 = busyManagerComponent.TryBeginBusy(_busyManager, "Test1", null)!;
            var token2 = busyManagerComponent.TryBeginBusy(_busyManager, "Test2", null)!;

            busyManagerComponent
                .TryGetToken(_busyManager, (component, token, arg3) => ReferenceEquals(token, token1), busyManagerComponent, DefaultMetadata)
                .ShouldEqual(token1);
            busyManagerComponent
                .TryGetToken(_busyManager, (component, token, arg3) => ReferenceEquals(token, token2), busyManagerComponent, DefaultMetadata)
                .ShouldEqual(token2);
            busyManagerComponent
                .TryGetToken(_busyManager, (component, token, arg3) => false, busyManagerComponent, DefaultMetadata)
                .ShouldBeNull();
        }

        [Fact]
        public void TryGetTokensShouldReturnAllTokens()
        {
            var busyManagerComponent = new BusyManagerComponent();
            using var t = _busyManager.AddComponent(busyManagerComponent);
            var token1 = busyManagerComponent.TryBeginBusy(_busyManager, "Test1", null)!;
            var token2 = busyManagerComponent.TryBeginBusy(_busyManager, "Test2", null)!;

            var tokens = busyManagerComponent.TryGetTokens(_busyManager, DefaultMetadata)!.AsList();
            tokens.Count.ShouldEqual(2);
            tokens.ShouldContain(token1);
            tokens.ShouldContain(token2);

            token1.Dispose();
            tokens = busyManagerComponent.TryGetTokens(_busyManager, DefaultMetadata)!.AsList();
            tokens.Count.ShouldEqual(1);
            tokens.ShouldContain(token2);

            token2.Dispose();
            tokens = busyManagerComponent.TryGetTokens(_busyManager, DefaultMetadata)!.AsList();
            (tokens == null || tokens.Count == 0).ShouldBeTrue();
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
            using var t = _busyManager.AddComponent(busyManagerComponent);
            IBusyToken? busyToken = null;
            var meta = includeMetadata ? DefaultMetadata : null;

            _listener.OnBeginBusy = (token, metadata) =>
            {
                ++count;
                metadata.ShouldEqual(meta);
                busyToken = token;
            };

            var beginBusy = delay == 0
                ? busyManagerComponent.TryBeginBusy(_busyManager, message, meta)!
                : busyManagerComponent.TryBeginBusy(_busyManager, new DelayBusyRequest(message, delay), meta)!;
            beginBusy.Message.ShouldEqual(message);

            if (delay == 0)
            {
                beginBusy.ShouldEqual(busyToken);
                count.ShouldEqual(1);
                return;
            }

            count.ShouldEqual(0);
            WaitCompletion(delay + 100);
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
            using var t = _busyManager.AddComponent(busyManagerComponent);
            var parentToken = busyManagerComponent.TryBeginBusy(_busyManager, message, null);
            IBusyToken? busyToken = null;
            var meta = includeMetadata ? DefaultMetadata : null;

            _listener.OnBeginBusy = (token, metadata) =>
            {
                ++count;
                metadata.ShouldEqual(meta);
                busyToken = token;
            };

            var beginBusy = delay == 0
                ? busyManagerComponent.TryBeginBusy(_busyManager, parentToken, meta)!
                : busyManagerComponent.TryBeginBusy(_busyManager, new DelayBusyRequest(parentToken, delay), meta)!;
            beginBusy.Message.ShouldEqual(message);

            if (delay == 0)
            {
                beginBusy.ShouldEqual(busyToken);
                count.ShouldEqual(1);
                return;
            }

            count.ShouldEqual(0);
            WaitCompletion(delay + 100);
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
            using var t = _busyManager.AddComponent(busyManagerComponent);
            _listener.OnBeginBusy = (token, metadata) => { ++count; };

            var beginBusy = busyManagerComponent.TryBeginBusy(_busyManager, new DelayBusyRequest(message, delay), null)!;
            beginBusy.Dispose();

            count.ShouldEqual(0);
            WaitCompletion(delay + 100);
            count.ShouldEqual(0);
        }

        [Theory]
        [InlineData(null, DefaultDelay)]
        [InlineData(1, DefaultDelay)]
        public void TryBeginBusyShouldIgnoreListenersDisposeBeforeDelayParentToken(object? message, int delay)
        {
            var count = 0;
            var busyManagerComponent = new BusyManagerComponent();
            using var t = _busyManager.AddComponent(busyManagerComponent);
            var parentToken = busyManagerComponent.TryBeginBusy(_busyManager, message, null);
            _listener.OnBeginBusy = (token, metadata) => { ++count; };

            var beginBusy = busyManagerComponent.TryBeginBusy(_busyManager, new DelayBusyRequest(parentToken, delay), null)!;
            beginBusy.Dispose();

            count.ShouldEqual(0);
            WaitCompletion(delay + 100);
            count.ShouldEqual(0);
        }

        [Theory]
        [InlineData(1, false)]
        [InlineData(10, false)]
        [InlineData(1, true)]
        [InlineData(10, true)]
        public void TokenShouldRegisterCallbacks(int callbackCount, bool unsubscribe)
        {
            var busyManagerComponent = new BusyManagerComponent();
            using var t = _busyManager.AddComponent(busyManagerComponent);
            var token = busyManagerComponent.TryBeginBusy(_busyManager, "Test", null)!;
            var completedCount = 0;
            var suspendedCount = 0;
            var callbacks = new ActionToken[callbackCount];

            for (var i = 0; i < callbackCount; i++)
            {
                var callback = new TestBusyTokenCallback
                {
                    OnCompleted = busyToken => { ++completedCount; },
                    OnSuspendChanged = b => { ++suspendedCount; }
                };
                callbacks[i] = token.RegisterCallback(callback);
            }

            if (unsubscribe)
            {
                callbackCount = 0;
                for (var i = 0; i < callbacks.Length; i++)
                    callbacks[i].Dispose();
            }

            suspendedCount.ShouldEqual(0);
            var actionToken = token.Suspend(this, DefaultMetadata);
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
            using var t = _busyManager.AddComponent(busyManagerComponent);
            var token = busyManagerComponent.TryBeginBusy(_busyManager, "Test", null)!;
            token.Dispose();
            var completedCount = 0;

            for (var i = 0; i < callbackCount; i++)
            {
                var callback = new TestBusyTokenCallback
                {
                    OnCompleted = busyToken => { ++completedCount; }
                };
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
            using var t = _busyManager.AddComponent(busyManagerComponent);
            var token = busyManagerComponent.TryBeginBusy(_busyManager, "Test", null)!;
            token.Suspend(this, DefaultMetadata);
            var suspendedCount = 0;

            for (var i = 0; i < callbackCount; i++)
            {
                var callback = new TestBusyTokenCallback
                {
                    OnSuspendChanged = b => { ++suspendedCount; }
                };
                token.RegisterCallback(callback);
            }

            suspendedCount.ShouldEqual(callbackCount);
        }
    }
}