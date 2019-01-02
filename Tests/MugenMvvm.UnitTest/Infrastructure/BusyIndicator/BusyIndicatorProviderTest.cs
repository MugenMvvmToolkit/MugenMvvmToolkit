using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Infrastructure.BusyIndicator;
using MugenMvvm.Interfaces.BusyIndicator;
using MugenMvvm.Interfaces.Models;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Infrastructure.BusyIndicator
{
    public class BusyIndicatorProviderTest : HasEventListenerUnitTestBase<IBusyIndicatorProviderListener>
    {
        #region Methods

        [Fact]
        public void BusyInfoShouldReturnNullByDefault()
        {
            var busyIndicator = GetBusyIndicator();
            busyIndicator.BusyInfo.ShouldBeNull();
        }

        [Fact]
        public void BeginWithMessageShouldUpdateBusyInfoNullMessage()
        {
            var busyIndicator = GetBusyIndicator();
            var token = busyIndicator.Begin(message: null);
            busyIndicator.BusyInfo.ShouldNotBeNull();
            busyIndicator.BusyInfo.Message.ShouldBeNull();
            token.IsCompleted.ShouldBeFalse();

            token.Dispose();
            token.IsCompleted.ShouldBeTrue();
            busyIndicator.BusyInfo.ShouldBeNull();
        }

        [Fact]
        public void BeginWithMessageShouldUpdateBusyInfoNullMessageNotNullDefault()
        {
            var defaultMsg = new object();
            var busyIndicator = GetBusyIndicator(defaultMsg);
            var token = busyIndicator.Begin(message: null);
            busyIndicator.BusyInfo.ShouldNotBeNull();
            busyIndicator.BusyInfo.Message.ShouldEqual(defaultMsg);
            token.IsCompleted.ShouldBeFalse();

            token.Dispose();
            token.IsCompleted.ShouldBeTrue();
            busyIndicator.BusyInfo.ShouldBeNull();
        }

        [Fact]
        public void BeginWithMessageShouldUpdateBusyInfoNotNullMessage()
        {
            var busyIndicator = GetBusyIndicator();
            var token = busyIndicator.Begin(message: busyIndicator);
            busyIndicator.BusyInfo.ShouldNotBeNull();
            busyIndicator.BusyInfo.Message.ShouldEqual(busyIndicator);
            token.IsCompleted.ShouldBeFalse();

            token.Dispose();
            token.IsCompleted.ShouldBeTrue();
            busyIndicator.BusyInfo.ShouldBeNull();
        }

        [Fact]
        public void BeginWithMessageShouldUpdateBusyInfoAfterDelay()
        {
            var busyIndicator = GetBusyIndicator();
            var token = busyIndicator.Begin(message: null, 1000);
            busyIndicator.BusyInfo.ShouldBeNull();

            Task.Delay(1100).Wait();

            busyIndicator.BusyInfo.ShouldNotBeNull();
            busyIndicator.BusyInfo.Message.ShouldBeNull();
            token.IsCompleted.ShouldBeFalse();

            token.Dispose();
            token.IsCompleted.ShouldBeTrue();
            busyIndicator.BusyInfo.ShouldBeNull();
        }

        [Fact]
        public void BeginWithTokenShouldUpdateBusyInfoNullMessage()
        {
            var rootIndicator = GetBusyIndicator();
            var busyToken = rootIndicator.Begin(message: null);

            var busyIndicator = GetBusyIndicator();
            var token = busyIndicator.Begin(busyToken);

            busyIndicator.BusyInfo.ShouldNotBeNull();
            busyIndicator.BusyInfo.Message.ShouldBeNull();
            token.IsCompleted.ShouldBeFalse();

            token.Dispose();
            token.IsCompleted.ShouldBeTrue();
            busyIndicator.BusyInfo.ShouldBeNull();
        }

        [Fact]
        public void BeginWithTokenShouldUpdateBusyInfoNotNullMessage()
        {
            var rootIndicator = GetBusyIndicator();
            var busyToken = rootIndicator.Begin(rootIndicator);

            var busyIndicator = GetBusyIndicator();
            var token = busyIndicator.Begin(busyToken);

            busyIndicator.BusyInfo.ShouldNotBeNull();
            busyIndicator.BusyInfo.Message.ShouldEqual(rootIndicator);
            token.IsCompleted.ShouldBeFalse();

            token.Dispose();
            token.IsCompleted.ShouldBeTrue();
            busyIndicator.BusyInfo.ShouldBeNull();
        }

        [Fact]
        public void BeginWithTokenMessageShouldUpdateBusyInfoAfterDelay()
        {
            var rootIndicator = GetBusyIndicator();
            var busyToken = rootIndicator.Begin(message: null);

            var busyIndicator = GetBusyIndicator();
            var token = busyIndicator.Begin(busyToken, 1000);
            busyIndicator.BusyInfo.ShouldBeNull();

            Task.Delay(1100).Wait();

            busyIndicator.BusyInfo.ShouldNotBeNull();
            busyIndicator.BusyInfo.Message.ShouldBeNull();
            token.IsCompleted.ShouldBeFalse();

            token.Dispose();
            token.IsCompleted.ShouldBeTrue();
            busyIndicator.BusyInfo.ShouldBeNull();
        }

        [Fact]
        public void BeginWithTokenShouldRemoveMessageAfterDisposeParentToken()
        {
            var rootIndicator = GetBusyIndicator();
            var busyToken = rootIndicator.Begin(message: null);

            var busyIndicator = GetBusyIndicator();
            busyIndicator.Begin(busyToken);

            busyIndicator.BusyInfo.ShouldNotBeNull();

            busyToken.Dispose();
            busyIndicator.BusyInfo.ShouldBeNull();
        }

        [Fact]
        public void GetTokensShouldReturnEmptyArrayAsDefaultValue()
        {
            var busyIndicator = GetBusyIndicator();
            busyIndicator.GetTokens().ShouldBeEmpty();
        }

        [Fact]
        public void GetTokensShouldReturnArrayOfTokens()
        {
            var rootIndicator = GetBusyIndicator();
            var busyToken = rootIndicator.Begin(message: null);

            var busyIndicator = GetBusyIndicator();
            var token1 = busyIndicator.Begin(busyToken);
            var list = busyIndicator.GetTokens();
            list.Count.ShouldEqual(1);
            list.ShouldContain(token1);


            var token2 = busyIndicator.Begin(message: null);
            list = busyIndicator.GetTokens();
            list.Count.ShouldEqual(2);
            list.ShouldContain(token1, token2);

            token1.Dispose();
            token2.Dispose();
            busyIndicator.GetTokens().ShouldBeEmpty();
        }

        [Fact]
        public void GetTokensShouldReturnArrayOfTokens1()
        {
            const int count = 1000;
            var busyTokens = new List<IBusyToken>();
            var busyIndicator = GetBusyIndicator();
            for (var i = 0; i < count; i++) busyTokens.Add(busyIndicator.Begin(i));

            var list = busyIndicator.GetTokens();
            list.Count.ShouldEqual(count);
            list.ShouldContain(busyTokens.ToArray());
        }

        [Fact]
        public void BeginShouldBeThreadSafe1()
        {
            const int count = 1000;
            var busyTokens = new List<IBusyToken>();
            var busyIndicator = GetBusyIndicator();
            var tasks = new List<Task>();
            for (var i = 0; i < count; i++)
            {
                object j = i;
                tasks.Add(Task.Run(() =>
                {
                    var busyToken = busyIndicator.Begin(j);
                    lock (busyTokens)
                    {
                        busyTokens.Add(busyToken);
                    }
                }));
            }

            Task.WaitAll(tasks.ToArray());

            var list = busyIndicator.GetTokens();
            list.Count.ShouldEqual(count);
            list.ShouldContain(busyTokens.ToArray());

            var array = list.Select(token => (int)token.Message).ToList();
            for (var i = 0; i < count; i++)
                array.ShouldContain(i);
        }

        [Fact]
        public void BeginShouldBeThreadSafe2()
        {
            const int count = 1000;
            var busyTokens = new List<IBusyToken>();
            var busyIndicator = GetBusyIndicator();
            for (var i = 0; i < count; i++) busyTokens.Add(busyIndicator.Begin(i));

            var tasks = new List<Task>();
            for (var i = 0; i < count / 2; i++)
            {
                var j = i;
                tasks.Add(Task.Run(() => busyTokens[j].Dispose()));
            }

            Task.WaitAll(tasks.ToArray());

            var list = busyIndicator.GetTokens();
            list.Count.ShouldEqual(count / 2);
            list.ShouldContain(busyTokens.Skip(count / 2).ToArray());

            var array = list.Select(token => (int)token.Message).ToList();
            for (var i = count / 2; i < count; i++)
                array.ShouldContain(i);
        }

        [Fact]
        public void BusyInfoTryGetMessageShouldReturnMessageByType()
        {
            const string message1 = "1";
            const string message3 = "3";
            var message2 = new TestBusyIndicatorProviderListener();

            var busyIndicator = GetBusyIndicator();
            busyIndicator.Begin(message1);
            busyIndicator.Begin(message2);
            busyIndicator.Begin(message3);

            busyIndicator.BusyInfo.TryGetMessage(out TestBusyIndicatorProviderListener message).ShouldBeTrue();
            message.ShouldEqual(message2);

            busyIndicator.BusyInfo.TryGetMessage(out List<object> _).ShouldBeFalse();
        }

        [Fact]
        public void BusyInfoTryGetMessageShouldReturnMessageByFilter()
        {
            const string message1 = "1";
            const string message3 = "3";
            var message2 = new TestBusyIndicatorProviderListener();

            var busyIndicator = GetBusyIndicator();
            busyIndicator.Begin(message1);
            busyIndicator.Begin(message2);
            busyIndicator.Begin(message3);

            busyIndicator.BusyInfo.TryGetMessage<string>(out var message, m => m == message1).ShouldBeTrue();
            message.ShouldEqual(message1);

            busyIndicator.BusyInfo.TryGetMessage(out message, m => m == string.Empty).ShouldBeFalse();
        }

        [Fact]
        public void BusyInfoGetMessagesShouldReturnMessages()
        {
            const string message1 = "1";
            const string message3 = "3";
            var message2 = new TestBusyIndicatorProviderListener();

            var busyIndicator = GetBusyIndicator();
            var t1 = busyIndicator.Begin(message1);
            busyIndicator.BusyInfo.GetMessages().ShouldContain(message1);
            var t2 = busyIndicator.Begin(message2);
            busyIndicator.BusyInfo.GetMessages().ShouldContain(message1, message2);
            var t3 = busyIndicator.Begin(message3);
            busyIndicator.BusyInfo.GetMessages().ShouldContain(message1, message2, message3);

            t1.Dispose();
            busyIndicator.BusyInfo.GetMessages().ShouldContain(message2, message3);

            t2.Dispose();
            busyIndicator.BusyInfo.GetMessages().ShouldContain(message3);

            t3.Dispose();
            busyIndicator.BusyInfo.ShouldBeNull();
        }

        [Fact]
        public void BusyTokenShouldUpdateCompletedState()
        {
            var busyIndicator = GetBusyIndicator();
            var t1 = busyIndicator.Begin(message: null);
            t1.IsCompleted.ShouldBeFalse();
            t1.Dispose();
            t1.IsCompleted.ShouldBeTrue();
        }

        [Fact]
        public void BusyTokenShouldNotifyListener()
        {
            IBusyToken completedToken = null;
            var listener = new TestBusyIndicatorProviderListener { OnCompleted = token => completedToken = token };
            var busyIndicator = GetBusyIndicator();

            var t1 = busyIndicator.Begin(message: null);
            t1.Register(listener);
            t1.Dispose();
            t1.ShouldEqual(completedToken);
        }

        [Fact]
        public void BusyTokenShouldNotifyListeners()
        {
            IBusyToken completedToken1 = null;
            IBusyToken completedToken2 = null;
            IBusyToken completedToken3 = null;
            var listener1 = new TestBusyIndicatorProviderListener { OnCompleted = token => completedToken1 = token };
            var listener2 = new TestBusyIndicatorProviderListener { OnCompleted = token => completedToken2 = token };
            var listener3 = new TestBusyIndicatorProviderListener { OnCompleted = token => completedToken3 = token };
            var busyIndicator = GetBusyIndicator();

            var t1 = busyIndicator.Begin(message: null);
            t1.Register(listener1);
            t1.Register(listener2);
            t1.Register(listener3);
            t1.Dispose();
            t1.ShouldEqual(completedToken1);
            t1.ShouldEqual(completedToken2);
            t1.ShouldEqual(completedToken3);
        }

        [Fact]
        public void BeginWithMessageShouldNotify()
        {
            string message1 = "1";
            string message2 = "2";
            var list = new List<IBusyInfo>();
            var listener = new TestBusyIndicatorProviderListener
            {
                OnBeginBusy = info =>
                {
                    list.Add(info);
                }
            };
            var busyIndicator = GetBusyIndicator();
            busyIndicator.AddListener(listener);

            list.ShouldBeEmpty();
            busyIndicator.Begin(message1);
            list.Count.ShouldEqual(1);
            list[0].Message.ShouldEqual(message1);

            list.Clear();
            busyIndicator.Begin(message2);
            list.Count.ShouldEqual(1);
            list[0].Message.ShouldEqual(message2);
        }

        [Fact]
        public void BeginWithTokenShouldNotify()
        {
            var rootIndicator = GetBusyIndicator();
            var busyToken1 = rootIndicator.Begin("1");
            var busyToken2 = rootIndicator.Begin("2");

            var list = new List<IBusyInfo>();
            var listener = new TestBusyIndicatorProviderListener
            {
                OnBeginBusy = info =>
                {
                    list.Add(info);
                }
            };
            var busyIndicator = GetBusyIndicator();
            busyIndicator.AddListener(listener);

            list.ShouldBeEmpty();
            busyIndicator.Begin(busyToken1);
            list.Count.ShouldEqual(1);
            list[0].Message.ShouldEqual(busyToken1.Message);

            list.Clear();
            busyIndicator.Begin(busyToken2);
            list.Count.ShouldEqual(1);
            list[0].Message.ShouldEqual(busyToken2.Message);
        }

        [Fact]
        public void ProviderShouldNotifyOnBusyChanged()
        {
            int notificationCount = 0;
            var listener = new TestBusyIndicatorProviderListener { OnBusyChanged = () => Interlocked.Increment(ref notificationCount) };

            var busyIndicator = GetBusyIndicator();
            busyIndicator.AddListener(listener);

            var t1 = busyIndicator.Begin(message: null);
            notificationCount.ShouldEqual(1);

            var t2 = busyIndicator.Begin(message: null);
            notificationCount.ShouldEqual(2);

            t2.Dispose();
            notificationCount.ShouldEqual(3);

            t1.Dispose();
            notificationCount.ShouldEqual(4);
        }

        [Fact]
        public void ProviderShouldNotifyOnBusyChangedParallel()
        {
            const int count = 1000;
            int notificationCount = 0;
            var listener = new TestBusyIndicatorProviderListener { OnBusyChanged = () => Interlocked.Increment(ref notificationCount) };

            var busyIndicator = GetBusyIndicator();
            busyIndicator.AddListener(listener);

            var tasks = new List<Task>();
            var tokens = new List<IBusyToken>();
            for (int i = 0; i < count; i++)
            {
                tasks.Add(Task.Run(() =>
                   {
                       lock (tokens)
                       {
                           tokens.Add(busyIndicator.Begin(message: null));
                       }
                   }));
            }

            Task.WaitAll(tasks.ToArray());
            notificationCount.ShouldEqual(count);

            notificationCount = 0;
            tasks.Clear();

            foreach (var busyToken in tokens)
            {
                var t = busyToken;
                tasks.Add(Task.Run(() =>
                {
                    t.Dispose();
                }));
            }

            Task.WaitAll(tasks.ToArray());
            notificationCount.ShouldEqual(count);
        }

        [Fact]
        public void BusyInfoShouldBeNullSuspend()
        {
            var busyIndicator = GetBusyIndicator();
            busyIndicator.Begin("");

            var busyInfo = busyIndicator.BusyInfo;
            busyInfo.ShouldNotBeNull();
            var suspendNotifications = busyIndicator.SuspendNotifications();
            busyIndicator.BusyInfo.ShouldBeNull();

            suspendNotifications.Dispose();
            busyIndicator.BusyInfo.ShouldEqual(busyInfo);
        }

        [Fact]
        public void BusyInfoShouldBeNullSuspendBegin()
        {
            var busyIndicator = GetBusyIndicator();

            busyIndicator.BusyInfo.ShouldBeNull();
            var suspendNotifications = busyIndicator.SuspendNotifications();

            busyIndicator.Begin("");
            busyIndicator.BusyInfo.ShouldBeNull();

            suspendNotifications.Dispose();
            busyIndicator.BusyInfo.ShouldNotBeNull();
        }

        [Fact]
        public void ProviderShouldNotNotifyOnBusyChangedSuspend()
        {
            var listener = new TestBusyIndicatorProviderListener
            {
                OnBusyChanged = () => { throw new NotSupportedException(); }
            };

            var busyIndicator = GetBusyIndicator();
            busyIndicator.AddListener(listener);

            var suspendNotifications = busyIndicator.SuspendNotifications();
            var t1 = busyIndicator.Begin(message: null);

            int notificationCount = 0;
            listener.OnBusyChanged = () => Interlocked.Increment(ref notificationCount);

            suspendNotifications.Dispose();
            notificationCount.ShouldEqual(1);
        }

        [Fact]
        public void BeginWithTokenShouldSuspendToken()
        {
            var rootIndicator = GetBusyIndicator();
            var busyToken = rootIndicator.Begin(message: null);

            var busyIndicator = GetBusyIndicator();
            var token = busyIndicator.Begin(busyToken);

            busyIndicator.BusyInfo.ShouldNotBeNull();
            var suspendNotifications = rootIndicator.SuspendNotifications();

            busyIndicator.BusyInfo.ShouldBeNull();
            suspendNotifications.Dispose();
            busyIndicator.BusyInfo.ShouldNotBeNull();
        }

        [Fact]
        public void BeginWithTokenShouldNotNotifyOnBusyChangedSuspend()
        {
            int notificationCount = 0;
            var listener = new TestBusyIndicatorProviderListener { OnBusyChanged = () => Interlocked.Increment(ref notificationCount) };

            var rootIndicator = GetBusyIndicator();
            var busyToken = rootIndicator.Begin(message: null);

            var busyIndicator = GetBusyIndicator();
            busyIndicator.AddListener(listener);
            var token = busyIndicator.Begin(busyToken);

            notificationCount.ShouldEqual(1);
            busyIndicator.BusyInfo.ShouldNotBeNull();

            notificationCount = 0;
            var suspendNotifications = rootIndicator.SuspendNotifications();

            notificationCount.ShouldEqual(1);
            busyIndicator.BusyInfo.ShouldBeNull();

            notificationCount = 0;
            suspendNotifications.Dispose();

            notificationCount.ShouldEqual(1);
            busyIndicator.BusyInfo.ShouldNotBeNull();
        }

        [Fact]
        public void BeginWithTokenShouldSuspendTokenParallel()
        {
            const int count = 1000;
            var providers = new List<IBusyIndicatorProvider>();
            var tokens = new List<IBusyToken>();
            var messages = new List<object>();

            for (int i = 0; i < count; i++)
            {
                object obj = i;
                var rootIndicator = GetBusyIndicator();
                var busyToken = rootIndicator.Begin(obj);
                providers.Add(rootIndicator);
                tokens.Add(busyToken);
                messages.Add(obj);
            }

            var busyIndicator = GetBusyIndicator();
            foreach (var busyToken in tokens)
                busyIndicator.Begin(busyToken);


            int notificationCount = 0;
            var listener = new TestBusyIndicatorProviderListener { OnBusyChanged = () => Interlocked.Increment(ref notificationCount) };
            busyIndicator.AddListener(listener);

            var tasks = new List<Task<IDisposable>>();
            foreach (var indicatorProvider in providers)
                tasks.Add(Task.Run(() => indicatorProvider.SuspendNotifications()));

            Task.WaitAll(tasks.ToArray());
            notificationCount.ShouldEqual(count);

            busyIndicator.BusyInfo.ShouldBeNull();

            notificationCount = 0;
            var disposables = tasks.Select(task => task.Result).ToList();
            tasks.Clear();
            foreach (var disposable in disposables)
            {
                var d = disposable;
                tasks.Add(Task.Run(() =>
                {
                    d.Dispose();
                    return d;
                }));
            }

            Task.WaitAll(tasks.ToArray());

            notificationCount.ShouldEqual(count);

            busyIndicator.BusyInfo.ShouldNotBeNull();

            var list = busyIndicator.BusyInfo.GetMessages();
            messages.ShouldContain(list);
        }

        [Fact]
        public void ClearBusyShouldClearAllBusy()
        {
            var busyIndicator = GetBusyIndicator();
            busyIndicator.Begin(message: null);
            busyIndicator.Begin(message: null);

            busyIndicator.GetTokens().Count.ShouldEqual(2);

            busyIndicator.ClearBusy();
            busyIndicator.GetTokens().Count.ShouldEqual(0);
            busyIndicator.BusyInfo.ShouldBeNull();
        }

        protected virtual IBusyIndicatorProvider GetBusyIndicator(object? defaultMessage = null)
        {
            return new BusyIndicatorProvider(defaultMessage);
        }

        protected override IBusyIndicatorProviderListener CreateListener()
        {
            return new TestBusyIndicatorProviderListener();
        }

        protected override IHasListeners<IBusyIndicatorProviderListener> GetHasEventListener()
        {
            return GetBusyIndicator();
        }

        #endregion

        #region Nested types

        private sealed class TestBusyIndicatorProviderListener : IBusyIndicatorProviderListener, IBusyTokenCallback
        {
            #region Properties

            public Action<IBusyInfo>? OnBeginBusy { get; set; }

            public Action? OnBusyChanged { get; set; }

            public Action<IBusyToken>? OnCompleted { get; set; }

            #endregion

            #region Implementation of interfaces

            void IBusyIndicatorProviderListener.OnBeginBusy(IBusyIndicatorProvider busyIndicatorProvider, IBusyInfo busyInfo)
            {
                OnBeginBusy?.Invoke(busyInfo);
            }

            void IBusyIndicatorProviderListener.OnBusyInfoChanged(IBusyIndicatorProvider busyIndicatorProvider)
            {
                OnBusyChanged?.Invoke();
            }

            void IBusyTokenCallback.OnCompleted(IBusyToken token)
            {
                OnCompleted?.Invoke(token);
            }

            public void OnSuspendChanged(bool suspended)
            {
            }

            #endregion
        }

        #endregion
    }
}