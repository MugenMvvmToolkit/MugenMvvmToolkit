using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Attributes;
using MugenMvvm.Components;
using MugenMvvm.Interfaces.BusyIndicator;
using MugenMvvm.Interfaces.BusyIndicator.Components;
using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.BusyIndicator
{
    public sealed class BusyIndicatorProvider : ComponentOwnerBase<IBusyIndicatorProvider>, IBusyIndicatorProvider
    {
        #region Fields

        private readonly object? _defaultBusyMessage;
        private BusyToken? _busyTail;
        private short _suspendCount;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public BusyIndicatorProvider(object? defaultBusyMessage = null, IComponentCollectionProvider? componentCollectionProvider = null)
            : base(componentCollectionProvider)
        {
            _defaultBusyMessage = defaultBusyMessage;
        }

        #endregion

        #region Properties

        public bool IsSuspended => _suspendCount != 0;

        public IBusyInfo? BusyInfo => _busyTail?.GetBusyInfo();

        private object Locker => this;

        #endregion

        #region Implementation of interfaces

        public IBusyToken Begin(IBusyToken parentToken, int millisecondsDelay = 0)
        {
            Should.NotBeNull(parentToken, nameof(parentToken));
            var busyToken = new BusyToken(this, parentToken);
            BeginBusyInternal(busyToken, millisecondsDelay);
            return busyToken;
        }

        public IBusyToken Begin(object? message, int millisecondsDelay = 0)
        {
            var busyToken = new BusyToken(this, message ?? _defaultBusyMessage);
            BeginBusyInternal(busyToken, millisecondsDelay);
            return busyToken;
        }

        public ActionToken Suspend()
        {
            bool? notify = null;
            lock (Locker)
            {
                if (++_suspendCount == 1)
                    notify = _busyTail?.SetSuspended(true);
            }

            if (notify.GetValueOrDefault())
                OnBusyInfoChanged(true);
            return new ActionToken((o, _) => ((BusyIndicatorProvider)o!).EndSuspendNotifications(), this);
        }

        public IReadOnlyList<IBusyToken> GetTokens()
        {
            return _busyTail?.GetTokens() ?? Default.EmptyArray<IBusyToken>();
        }

        public void Dispose()
        {
            var busyTokens = _busyTail?.GetTokens();
            if (busyTokens == null)
                return;
            for (var index = 0; index < busyTokens.Count; index++)
                busyTokens[index].Dispose();
            this.ClearComponents();
        }

        #endregion

        #region Methods

        private void BeginBusyInternal(BusyToken busyToken, int millisecondsDelay)
        {
            if (millisecondsDelay != 0)
            {
                Task.Delay(millisecondsDelay)
                    .ContinueWith((_, state) =>
                    {
                        var token = (BusyToken)state;
                        token.Provider.BeginBusyInternal(token, 0);
                    }, busyToken, TaskContinuationOptions.ExecuteSynchronously);
                return;
            }

            if (busyToken.Combine())
                OnBeginBusy(busyToken);
        }

        private void EndSuspendNotifications()
        {
            bool? notify = null;
            lock (Locker)
            {
                if (--_suspendCount == 0)
                    notify = _busyTail?.SetSuspended(false);
            }

            if (notify.GetValueOrDefault())
                OnBusyInfoChanged();
        }

        private void OnBeginBusy(IBusyInfo busyInfo)
        {
            var items = GetComponents<IBusyIndicatorProviderListener>(null);
            for (var i = 0; i < items.Length; i++)
                items[i].OnBeginBusy(this, busyInfo);
        }

        private void OnBusyInfoChanged(bool ignoreSuspend = false)
        {
            if (!ignoreSuspend && IsSuspended)
                return;
            var items = GetComponents<IBusyIndicatorProviderListener>(null);
            for (var i = 0; i < items.Length; i++)
                items[i].OnBusyInfoChanged(this);
        }

        #endregion

        #region Nested types

        private sealed class BusyToken : IBusyToken, IBusyTokenCallback, IBusyInfo
        {
            #region Fields

            public readonly BusyIndicatorProvider Provider;

            private IBusyTokenCallback[]? _listeners;
            private BusyToken? _next;
            private BusyToken? _prev;
            private bool _suspended;
            private bool _suspendedExternal;
            private int _suspendExternalCount;

            #endregion

            #region Constructors

            public BusyToken(BusyIndicatorProvider provider, object? message)
            {
                Message = message;
                Provider = provider;
            }

            public BusyToken(BusyIndicatorProvider provider, IBusyToken token)
            {
                token.Register(this);
                Message = token.Message;
                Provider = provider;
            }

            #endregion

            #region Properties

            public bool IsCompleted => ReferenceEquals(Default.EmptyArray<IBusyTokenCallback>(), _listeners);

            public object? Message { get; }

            public bool IsSuspended => _suspended || _suspendedExternal;

            public IBusyToken Token => this;

            private object Locker => Provider.Locker;

            #endregion

            #region Implementation of interfaces

            public IBusyToken? TryGetToken(Func<IBusyToken, bool> filter)
            {
                Should.NotBeNull(filter, nameof(filter));
                lock (Locker)
                {
                    var token = Provider._busyTail;
                    while (token != null)
                    {
                        if (filter(token))
                            return token;
                        token = token._prev;
                    }
                }

                return null;
            }

            public IReadOnlyList<IBusyToken> GetTokens()
            {
                List<IBusyToken>? tokens = null;
                lock (Locker)
                {
                    var token = Provider._busyTail;
                    while (token != null)
                    {
                        if (tokens == null)
                            tokens = new List<IBusyToken>();
                        tokens.Insert(0, token);
                        token = token._prev;
                    }
                }

                return tokens ?? (IReadOnlyList<IBusyToken>)Default.EmptyArray<IBusyToken>();
            }

            public void Register(IBusyTokenCallback callback)
            {
                Should.NotBeNull(callback, nameof(callback));
                if (IsCompleted)
                {
                    callback.OnCompleted(this);
                    return;
                }

                lock (Locker)
                {
                    if (!IsCompleted)
                    {
                        if (_listeners == null)
                            _listeners = new[] { callback };
                        else
                        {
                            var listeners = new IBusyTokenCallback[_listeners.Length + 1];
                            Array.Copy(_listeners, listeners, _listeners.Length);
                            listeners[listeners.Length - 1] = callback;
                            _listeners = listeners;
                        }

                        if (IsSuspended)
                            callback.OnSuspendChanged(true);
                        return;
                    }
                }

                callback.OnCompleted(this);
            }

            public ActionToken Suspend()
            {
                return SuspendExternal(true);
            }

            public void Dispose()
            {
                if (Provider == null)
                {
                    _listeners = Default.EmptyArray<IBusyTokenCallback>();
                    return;
                }

                IBusyTokenCallback[]? listeners;
                lock (Locker)
                {
                    listeners = _listeners;
                    if (_prev != null)
                        _prev._next = _next;
                    if (_next == null)
                        Provider._busyTail = _prev;
                    else
                        _next._prev = _prev;
                    _listeners = Default.EmptyArray<IBusyTokenCallback>();
                }

                if (listeners != null)
                {
                    for (var i = 0; i < listeners.Length; i++)
                        listeners[i].OnCompleted(this);
                }

                Provider.OnBusyInfoChanged();
            }

            public void OnCompleted(IBusyToken token)
            {
                Dispose();
            }

            public void OnSuspendChanged(bool suspended)
            {
                if (suspended)
                    SuspendExternal(false);
                else
                    OnEndSuspendExternal();
            }

            #endregion

            #region Methods

            public IBusyInfo? GetBusyInfo()
            {
                //current
                if (!IsSuspended)
                    return this;

                //prev
                var token = _prev;
                while (token != null)
                {
                    if (!token.IsSuspended)
                        return token;
                    token = token._prev;
                }

                return null;
            }

            public bool SetSuspended(bool suspended)
            {
                //current
                var result = SetSuspendedInternal(suspended);

                //prev
                var token = _prev;
                while (token != null)
                {
                    if (token.SetSuspendedInternal(suspended))
                        result = true;
                    token = token._prev;
                }

                return result;
            }

            public bool Combine()
            {
                if (IsCompleted)
                    return false;
                lock (Locker)
                {
                    if (IsCompleted)
                        return false;
                    if (Provider._busyTail != null)
                    {
                        _prev = Provider._busyTail;
                        Provider._busyTail._next = this;
                    }

                    Provider._busyTail = this;
                    SetSuspendedInternal(Provider.IsSuspended);
                }

                Provider.OnBusyInfoChanged();
                return true;
            }

            private ActionToken SuspendExternal(bool withToken)
            {
                if (Interlocked.Increment(ref _suspendExternalCount) == 1)
                    SetSuspendedExternal(true);

                if (withToken)
                    return new ActionToken((t, _) => ((BusyToken)t!).OnEndSuspendExternal(), this);
                return default;
            }

            private void OnEndSuspendExternal()
            {
                if (Interlocked.Decrement(ref _suspendExternalCount) == 0)
                    SetSuspendedExternal(false);
            }

            private bool SetSuspendedInternal(bool suspended)
            {
                return SetSuspended(ref _suspended, suspended);
            }

            private void SetSuspendedExternal(bool suspended)
            {
                if (Provider == null)
                    SetSuspended(ref _suspendedExternal, suspended);
                else
                {
                    bool notify;
                    lock (Locker)
                    {
                        notify = SetSuspended(ref _suspendedExternal, suspended);
                    }

                    if (notify)
                        Provider.OnBusyInfoChanged();
                }
            }

            private bool SetSuspended(ref bool field, bool suspended)
            {
                if (IsCompleted)
                    return false;
                if (field == suspended)
                    return false;
                var oldValue = IsSuspended;
                field = suspended;
                var changed = oldValue != IsSuspended;
                if (changed)
                {
                    var items = _listeners;
                    if (items != null)
                    {
                        for (var i = 0; i < items.Length; i++)
                            items[i]?.OnSuspendChanged(suspended);
                    }
                }

                return changed;
            }

            #endregion
        }

        #endregion
    }
}