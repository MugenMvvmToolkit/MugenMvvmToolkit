using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Attributes;
using MugenMvvm.Collections;
using MugenMvvm.Infrastructure.Internal;
using MugenMvvm.Interfaces.BusyIndicator;
using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Infrastructure.BusyIndicator
{
    public sealed class BusyIndicatorProvider : IBusyIndicatorProvider
    {
        #region Fields

        private readonly object? _defaultBusyMessage;
        private BusyToken? _busyTail;
        private int _suspendCount;
        private readonly object _locker;
        private IComponentCollection<IBusyIndicatorProviderListener>? _listeners;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public BusyIndicatorProvider(IComponentCollection<IBusyIndicatorProviderListener>? listeners = null, object? defaultBusyMessage = null)
        {
            _listeners = listeners;
            _defaultBusyMessage = defaultBusyMessage;
            _locker = this;
        }

        #endregion

        #region Properties

        public bool IsSuspended => _suspendCount != 0;

        public IComponentCollection<IBusyIndicatorProviderListener> Listeners
        {
            get
            {
                if (_listeners == null)
                    MugenExtensions.LazyInitialize(ref _listeners, this);
                return _listeners;
            }
        }

        public IBusyInfo? BusyInfo => _busyTail?.GetBusyInfo();

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

        public IDisposable Suspend()
        {
            bool? notify = null;
            lock (_locker)
            {
                if (++_suspendCount == 1)
                    notify = _busyTail?.SetSuspended(true);
            }
            if (notify.GetValueOrDefault())
                OnBusyInfoChanged(true);
            return WeakActionToken.Create(this, @base => @base.EndSuspendNotifications());
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
            _listeners?.Clear();
        }

        #endregion

        #region Methods

        private void BeginBusyInternal(BusyToken busyToken, int millisecondsDelay)
        {
            if (millisecondsDelay != 0)
            {
                Task.Delay(millisecondsDelay).ContinueWith(task => BeginBusyInternal(busyToken, 0));
                return;
            }
            if (busyToken.Combine())
                OnBeginBusy(busyToken);
        }

        private void EndSuspendNotifications()
        {
            bool? notify = null;
            lock (_locker)
            {
                if (--_suspendCount == 0)
                    notify = _busyTail?.SetSuspended(false);
            }
            if (notify.GetValueOrDefault())
                OnBusyInfoChanged();
        }

        private void OnBeginBusy(IBusyInfo busyInfo)
        {
            var items = GetListeners();
            for (var i = 0; i < items.Count; i++)
                items[i].OnBeginBusy(this, busyInfo);
        }

        private void OnBusyInfoChanged(bool ignoreSuspend = false)
        {
            if (!ignoreSuspend && IsSuspended)
                return;
            var items = GetListeners();
            for (var i = 0; i < items.Count; i++)
                items[i].OnBusyInfoChanged(this);
        }

        private IReadOnlyList<IBusyIndicatorProviderListener> GetListeners()
        {
            return _listeners?.GetItems() ?? Default.EmptyArray<IBusyIndicatorProviderListener>();
        }

        #endregion

        #region Nested types

        private sealed class BusyToken : IBusyToken, IBusyTokenCallback, IBusyInfo
        {
            #region Fields

            private readonly BusyIndicatorProvider _provider;

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
                _provider = provider;
            }

            public BusyToken(BusyIndicatorProvider provider, IBusyToken token)
            {
                token.Register(this);
                Message = token.Message;
                _provider = provider;
            }

            #endregion

            #region Properties

            public bool IsCompleted => ReferenceEquals(Default.EmptyArray<IBusyTokenCallback>(), _listeners);

            public object? Message { get; }

            public bool IsSuspended => _suspended || _suspendedExternal;

            public IBusyToken Token => this;

            private object Locker => _provider._locker;

            #endregion

            #region Implementation of interfaces

            public IBusyToken? TryGetToken(Func<IBusyToken, bool> filter)
            {
                Should.NotBeNull(filter, nameof(filter));
                lock (Locker)
                {
                    var token = _provider._busyTail;
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
                    var token = _provider._busyTail;
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

            public IDisposable Suspend()
            {
                return SuspendExternal(true)!;
            }

            public void Dispose()
            {
                if (_provider == null)
                {
                    _listeners = Default.EmptyArray<IBusyTokenCallback>();
                    return;
                }

                IBusyTokenCallback[]? listeners = null;
                lock (Locker)
                {
                    listeners = _listeners;
                    if (_prev != null)
                        _prev._next = _next;
                    if (_next == null)
                        _provider._busyTail = _prev;
                    else
                        _next._prev = _prev;
                    _listeners = Default.EmptyArray<IBusyTokenCallback>();
                }

                if (listeners != null)
                {
                    for (var i = 0; i < listeners.Length; i++)
                        listeners[i].OnCompleted(this);
                }

                _provider.OnBusyInfoChanged();
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
                bool result = SetSuspendedInternal(suspended);

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
                    if (_provider._busyTail != null)
                    {
                        _prev = _provider._busyTail;
                        _provider._busyTail._next = this;
                    }

                    _provider._busyTail = this;
                    SetSuspendedInternal(_provider.IsSuspended);
                }

                _provider.OnBusyInfoChanged();
                return true;
            }

            private IDisposable? SuspendExternal(bool withToken)
            {
                if (Interlocked.Increment(ref _suspendExternalCount) == 1)
                    SetSuspendedExternal(true);

                if (withToken)
                    return WeakActionToken.Create(this, t => t.OnEndSuspendExternal());
                return null;
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
                if (_provider == null)
                    SetSuspended(ref _suspendedExternal, suspended);
                else
                {
                    bool notify;
                    lock (Locker)
                    {
                        notify = SetSuspended(ref _suspendedExternal, suspended);
                    }
                    if (notify)
                        _provider.OnBusyInfoChanged();
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
                        for (int i = 0; i < items.Length; i++)
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