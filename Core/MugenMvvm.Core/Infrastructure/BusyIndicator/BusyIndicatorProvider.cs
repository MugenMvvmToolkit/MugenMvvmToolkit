using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Collections;
using MugenMvvm.Interfaces.BusyIndicator;

namespace MugenMvvm.Infrastructure.BusyIndicator
{
    public class BusyIndicatorProvider : IBusyIndicatorProvider
    {
        #region Fields

        private readonly object? _defaultBusyMessage;
        private BusyToken? _busyTail;

        private ArrayListLight<IBusyIndicatorProviderListener>? _subscribers;
        private int _suspendCount;

        #endregion

        #region Constructors

        public BusyIndicatorProvider(object? defaultBusyMessage = null)
        {
            _defaultBusyMessage = defaultBusyMessage;
        }

        #endregion

        #region Properties

        public bool IsNotificationsSuspended => _suspendCount != 0;

        public IBusyInfo? BusyInfo => _suspendCount == 0 ? _busyTail : null;

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

        public IDisposable SuspendNotifications()
        {
            if (Interlocked.Increment(ref _suspendCount) == 1)
            {
                var busyTail = _busyTail;
                if (busyTail != null)
                    OnBusyInfoChanged(true);
            }
            return WeakActionToken.Create(this, @base => @base.EndSuspendNotifications());
        }

        public IReadOnlyList<IBusyToken> GetTokens()
        {
            var tail = _busyTail;
            if (tail == null)
                return Default.EmptyArray<IBusyToken>();
            return tail.GetTokens();
        }

        public void AddListener(IBusyIndicatorProviderListener listener)
        {
            Should.NotBeNull(listener, nameof(listener));
            if (_subscribers == null)
                MugenExtensions.LazyInitialize(ref _subscribers, new ArrayListLight<IBusyIndicatorProviderListener>());
            _subscribers!.AddWithLock(listener);
        }

        public void RemoveListener(IBusyIndicatorProviderListener listener)
        {
            Should.NotBeNull(listener, nameof(listener));
            _subscribers?.RemoveWithLock(listener);
        }

        public IReadOnlyList<IBusyIndicatorProviderListener> GetListeners()
        {
            if (_subscribers == null)
                return Default.EmptyArray<IBusyIndicatorProviderListener>();
            var items = _subscribers.GetItemsWithLock(out var size);
            var listeners = new IBusyIndicatorProviderListener[size];
            for (int i = 0; i < size; i++)
                listeners[i] = items[i];
            return listeners;
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
            if (Interlocked.Decrement(ref _suspendCount) == 0)
            {
                var busyTail = _busyTail;
                if (busyTail != null)
                    OnBusyInfoChanged();
            }
        }

        private void OnBeginBusy(IBusyInfo busyInfo)
        {
            var items = _subscribers?.GetItems(out _);
            if (items != null)
            {
                for (var i = 0; i < items.Length; i++)
                    items[i]?.OnBeginBusy(busyInfo);
            }
        }

        private void OnBusyInfoChanged(bool ignoreSuspend = false)
        {
            if (!ignoreSuspend && IsNotificationsSuspended)
                return;
            var items = _subscribers?.GetItems(out _);
            if (items != null)
            {
                for (var i = 0; i < items.Length; i++)
                    items[i]?.OnBusyInfoChanged();
            }
        }

        #endregion

        #region Nested types

        private sealed class BusyToken : IBusyToken, IBusyTokenCallback, IBusyInfo
        {
            #region Fields

            private readonly BusyIndicatorProvider _provider;

            private ArrayListLight<IBusyTokenCallback>? _listeners;
            private BusyToken? _next;
            private BusyToken? _prev;

            private static readonly ArrayListLight<IBusyTokenCallback> CompletedList;

            #endregion

            #region Constructors

            static BusyToken()
            {
                CompletedList = new ArrayListLight<IBusyTokenCallback>(1);
            }

            public BusyToken(BusyIndicatorProvider provider, object? message)
            {
                Message = message;
                _provider = provider;
            }

            public BusyToken(BusyIndicatorProvider provider, IBusyToken token)
                : this(provider, token.Message)
            {
                token.Register(this);
            }

            #endregion

            #region Properties

            public bool IsCompleted => ReferenceEquals(CompletedList, _listeners);

            public object? Message { get; }

            #endregion

            #region Implementation of interfaces

            public bool TryGetMessage<TType>(out TType message, Func<TType, bool>? filter = null)
            {
                lock (_provider)
                {
                    //Prev
                    var token = _prev;
                    while (token != null)
                    {
                        if (TryGetMessage(token, filter, out message))
                            return true;
                        token = token._prev;
                    }

                    if (TryGetMessage(this, filter, out message))
                        return true;

                    //Next
                    token = _next;
                    while (token != null)
                    {
                        if (TryGetMessage(token, filter, out message))
                            return true;
                        token = token._next;
                    }
                }

                return false;
            }

            public IReadOnlyList<object?> GetMessages()
            {
                var list = new List<object?>();
                lock (_provider)
                {
                    //Prev
                    var token = _prev;
                    while (token != null)
                    {
                        list.Insert(0, token.Message);
                        token = token._prev;
                    }

                    list.Add(Message);

                    //Next
                    token = _next;
                    while (token != null)
                    {
                        list.Add(token.Message);
                        token = token._next;
                    }
                }

                return list;
            }

            public void Register(IBusyTokenCallback callback)
            {
                if (IsCompleted)
                {
                    callback.OnCompleted(this);
                    return;
                }

                lock (_provider)
                {
                    if (!IsCompleted)
                    {
                        if (_listeners == null)
                            _listeners = new ArrayListLight<IBusyTokenCallback>(2);
                        _listeners.Add(callback);
                        return;
                    }
                }

                callback.OnCompleted(this);
            }

            public void Dispose()
            {
                IBusyTokenCallback[]? listeners = null;
                lock (_provider)
                {
                    listeners = _listeners?.ToArray();
                    if (_prev != null)
                        _prev._next = _next;
                    if (_next == null)
                        _provider._busyTail = _prev;
                    else
                        _next._prev = _prev;
                    _listeners = CompletedList;
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

            #endregion

            #region Methods

            public bool Combine()
            {
                if (IsCompleted)
                    return false;
                lock (_provider)
                {
                    if (IsCompleted)
                        return false;
                    if (_provider._busyTail != null)
                    {
                        _prev = _provider._busyTail;
                        _provider._busyTail._next = this;
                    }

                    _provider._busyTail = this;
                }

                _provider.OnBusyInfoChanged();
                return true;
            }

            public IReadOnlyList<IBusyToken> GetTokens()
            {
                List<IBusyToken>? tokens = null;
                lock (_provider)
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

            private static bool TryGetMessage<TType>(BusyToken token, Func<TType, bool>? filter, out TType result)
            {
                if (token.Message is TType msg)
                {
                    result = msg;
                    if (filter == null || filter(result))
                        return true;
                }

                result = default!;
                return false;
            }

            #endregion
        }

        #endregion
    }
}