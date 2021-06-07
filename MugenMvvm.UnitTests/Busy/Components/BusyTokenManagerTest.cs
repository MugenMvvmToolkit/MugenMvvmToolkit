using System.Collections.Generic;
using MugenMvvm.Busy;
using MugenMvvm.Busy.Components;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Busy;
using MugenMvvm.Internal;
using MugenMvvm.Tests.Busy;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Busy.Components
{
    public class BusyTokenManagerTest : UnitTestBase
    {
        private const int DefaultDelay = 150;

        private readonly TestBusyManagerListener _listener;
        private readonly BusyTokenManager _busyTokenManager;

        public BusyTokenManagerTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            _listener = new TestBusyManagerListener();
            _busyTokenManager = new BusyTokenManager();
            BusyManager.AddComponent(_busyTokenManager);
            BusyManager.AddComponent(_listener);
        }

        [Fact]
        public void DisposeTokenShouldNotifyListeners()
        {
            var count = 0;
            var busyToken = BusyManager.TryBeginBusy(this)!;
            busyToken.Owner.ShouldEqual(BusyManager);

            _listener.OnBusyChanged = (_, _) => { ++count; };
            count.ShouldEqual(0);
            busyToken.Dispose();
            count.ShouldEqual(1);
        }

        [Fact]
        public void OnAttachShouldNotifyListeners()
        {
            var count = 0;
            BusyManager.RemoveComponent(_busyTokenManager);
            _listener.OnBusyChanged = (_, _) => { ++count; };

            count.ShouldEqual(0);
            BusyManager.AddComponent(_busyTokenManager);
            count.ShouldEqual(1);
        }

        [Fact]
        public void OnDetachShouldNotifyListeners()
        {
            var count = 0;
            _listener.OnBusyChanged = (_, _) => { ++count; };
            count.ShouldEqual(0);
            BusyManager.RemoveComponent(_busyTokenManager);
            count.ShouldEqual(1);
            BusyManager.AddComponent(_busyTokenManager);
        }

        [Fact]
        public void SuspendTokenShouldNotifyListeners()
        {
            var count = 0;
            var busyToken = BusyManager.TryBeginBusy(this)!;

            _listener.OnBusyChanged = (_, _) => { ++count; };
            count.ShouldEqual(0);
            var actionToken = busyToken.Suspend(this, DefaultMetadata);
            count.ShouldEqual(1);
            actionToken.Dispose();
            count.ShouldEqual(2);
        }

        [Fact]
        public void TokenShouldUpdateCompletedState()
        {
            var token = BusyManager.TryBeginBusy(this)!;
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
            var parentToken = BusyManager.TryBeginBusy(this)!;
            var token = BusyManager.TryBeginBusy(parentToken)!;
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
            var parentToken = BusyManager.TryBeginBusy(this)!;
            var token = BusyManager.TryBeginBusy(parentToken)!;
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
            var token = BusyManager.TryBeginBusy(this)!;
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
            var parentToken = BusyManager.TryBeginBusy(this)!;
            var token = BusyManager.TryBeginBusy(parentToken)!;
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
            var parentToken = BusyManager.TryBeginBusy(this)!;
            var token = BusyManager.TryBeginBusy(parentToken)!;
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
            _listener.OnBusyChanged = (_, _) => { ++count; };
            count.ShouldEqual(0);
            BusyManager.TryBeginBusy(this);
            count.ShouldEqual(1);
        }

        [Fact]
        public void TryGetTokenShouldReturnAllTokens()
        {
            var token1 = BusyManager.TryBeginBusy("Test1")!;
            var token2 = BusyManager.TryBeginBusy("Test2")!;
            var tokens = new List<IBusyToken>();

            BusyManager.TryGetToken(this, (component, token, arg3) =>
            {
                this.ShouldEqual(component);
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
            var token1 = BusyManager.TryBeginBusy("Test1")!;
            var token2 = BusyManager.TryBeginBusy("Test2")!;

            BusyManager
                .TryGetToken(BusyManager, (component, token, arg3) => ReferenceEquals(token, token1), DefaultMetadata)
                .ShouldEqual(token1);
            BusyManager
                .TryGetToken(BusyManager, (component, token, arg3) => ReferenceEquals(token, token2), DefaultMetadata)
                .ShouldEqual(token2);
            BusyManager
                .TryGetToken(BusyManager, (component, token, arg3) => false, DefaultMetadata)
                .ShouldBeNull();
        }

        [Fact]
        public void TryGetTokensShouldReturnAllTokens()
        {
            var token1 = BusyManager.TryBeginBusy("Test1")!;
            var token2 = BusyManager.TryBeginBusy("Test2")!;

            var tokens = BusyManager.GetTokens(DefaultMetadata)!.AsList();
            tokens.Count.ShouldEqual(2);
            tokens.ShouldContain(token1);
            tokens.ShouldContain(token2);

            token1.Dispose();
            tokens = BusyManager.GetTokens(DefaultMetadata)!.AsList();
            tokens.Count.ShouldEqual(1);
            tokens.ShouldContain(token2);

            token2.Dispose();
            tokens = BusyManager.GetTokens(DefaultMetadata)!.AsList();
            (tokens == null || tokens.Count == 0).ShouldBeTrue();
        }

        protected override IBusyManager GetBusyManager() => new BusyManager(ComponentCollectionManager);

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
            IBusyToken? busyToken = null;
            var meta = includeMetadata ? DefaultMetadata : null;

            _listener.OnBeginBusy = (o, token, metadata) =>
            {
                ++count;
                o.ShouldEqual(BusyManager);
                metadata.ShouldEqual(meta);
                busyToken = token;
            };

            var beginBusy = delay == 0
                ? BusyManager.TryBeginBusy(message, meta)!
                : BusyManager.TryBeginBusy(new DelayBusyRequest(message, delay), meta)!;
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
            var parentToken = BusyManager.TryBeginBusy(message);
            IBusyToken? busyToken = null;
            var meta = includeMetadata ? DefaultMetadata : null;

            _listener.OnBeginBusy = (o, token, metadata) =>
            {
                ++count;
                o.ShouldEqual(BusyManager);
                metadata.ShouldEqual(meta);
                busyToken = token;
            };

            var beginBusy = delay == 0
                ? BusyManager.TryBeginBusy(parentToken, meta)!
                : BusyManager.TryBeginBusy(new DelayBusyRequest(parentToken, delay), meta)!;
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
            _listener.OnBeginBusy = (_, _, _) => { ++count; };

            var beginBusy = BusyManager.TryBeginBusy(new DelayBusyRequest(message, delay))!;
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
            var parentToken = BusyManager.TryBeginBusy(message);
            _listener.OnBeginBusy = (_, _, _) => { ++count; };

            var beginBusy = BusyManager.TryBeginBusy(new DelayBusyRequest(parentToken, delay))!;
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
            var token = BusyManager.TryBeginBusy("Test")!;
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
            var token = BusyManager.TryBeginBusy("Test")!;
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
            var token = BusyManager.TryBeginBusy("Test")!;
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