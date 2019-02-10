using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MugenMvvm.Attributes;
using MugenMvvm.Collections;
using MugenMvvm.Infrastructure.Internal;
using MugenMvvm.Interfaces.BusyIndicator;

namespace MugenMvvm.Infrastructure.BusyIndicator
{
    public class BusyIndicatorProvider : HasListenersBase<IBusyIndicatorProviderListener>, IBusyIndicatorProvider
    {
        #region Fields

        private readonly object? _defaultBusyMessage;
        private BusyToken? _busyTail;
        private int _suspendCount;
        private readonly object _locker;
        internal IBusyIndicatorProviderListener? InternalListener;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public BusyIndicatorProvider(object? defaultBusyMessage = null)
        {
            _defaultBusyMessage = defaultBusyMessage;
            _locker = this;
        }

        #endregion

        #region Properties

        public bool IsNotificationsSuspended => _suspendCount != 0;

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

        public IDisposable SuspendNotifications()
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
            var tail = _busyTail;
            if (tail == null)
                return Default.EmptyArray<IBusyToken>();
            return tail.GetTokens();
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
            InternalListener?.OnBeginBusy(this, busyInfo);
            var items = GetListenersInternal();
            if (items != null)
            {
                for (var i = 0; i < items.Length; i++)
                    items[i]?.OnBeginBusy(this, busyInfo);
            }
        }

        private void OnBusyInfoChanged(bool ignoreSuspend = false)
        {
            if (!ignoreSuspend && IsNotificationsSuspended)
                return;
            InternalListener?.OnBusyInfoChanged(this);
            var items = GetListenersInternal();
            if (items != null)
            {
                for (var i = 0; i < items.Length; i++)
                    items[i]?.OnBusyInfoChanged(this);
            }
        }

        #endregion

        #region Nested types

        private sealed class BusyToken : IBusyToken, IBusyTokenCallback, IBusyInfo
        {
            #region Fields

            private readonly BusyIndicatorProvider _provider;

            private LightArrayList<IBusyTokenCallback>? _listeners;
            private BusyToken? _next;
            private BusyToken? _prev;
            private bool _suspended;
            private bool _suspendedExternal;

            private static readonly LightArrayList<IBusyTokenCallback> CompletedList;

            #endregion

            #region Constructors

            static BusyToken()
            {
                CompletedList = new LightArrayList<IBusyTokenCallback>(1);
            }

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

            public bool IsCompleted => ReferenceEquals(CompletedList, _listeners);

            public object? Message { get; }

            private bool IsSuspended => _suspended || _suspendedExternal;

            private object Locker => _provider._locker;

            #endregion

            #region Implementation of interfaces

            public bool TryGetMessage<TType>(out TType message, Func<TType, bool>? filter = null)
            {
                lock (Locker)
                {
                    //prev
                    var token = _prev;
                    while (token != null)
                    {
                        if (TryGetMessage(token, filter, out message))
                            return true;
                        token = token._prev;
                    }

                    //current
                    if (TryGetMessage(this, filter, out message))
                        return true;

                    //next
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
                lock (Locker)
                {
                    //prev
                    var token = _prev;
                    while (token != null)
                    {
                        list.Insert(0, token.Message);
                        token = token._prev;
                    }

                    //current
                    list.Add(Message);

                    //next
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
                            _listeners = new LightArrayList<IBusyTokenCallback>(2);
                        _listeners.Add(callback);
                        callback.OnSuspendChanged(IsSuspended);
                        return;
                    }
                }

                callback.OnCompleted(this);
            }

            public void Dispose()
            {
                if (_provider == null)
                {
                    _listeners = CompletedList;
                    return;
                }

                IBusyTokenCallback[]? listeners = null;
                lock (Locker)
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

            public void OnSuspendChanged(bool suspended)
            {
                if (_provider == null)
                {
                    SetSuspendedExternal(suspended);
                    return;
                }

                bool notify;
                lock (Locker)
                {
                    notify = SetSuspendedExternal(suspended);
                }
                if (notify)
                    _provider.OnBusyInfoChanged();
            }

            #endregion

            #region Methods

            public IBusyInfo? GetBusyInfo()
            {
                //prev
                var token = _prev;
                while (token != null)
                {
                    if (!token.IsSuspended)
                        return token;
                    token = token._prev;
                }

                //current
                if (!IsSuspended)
                    return this;

                //next
                token = _next;
                while (token != null)
                {
                    if (!token.IsSuspended)
                        return token;
                    token = token._next;
                }

                return null;
            }

            public bool SetSuspended(bool suspended)
            {
                bool result = false;
                //prev
                var token = _prev;
                while (token != null)
                {
                    if (token.SetSuspendedInternal(suspended))
                        result = true;
                    token = token._prev;
                }

                //current
                if (SetSuspendedInternal(suspended))
                    result = true;

                //next
                token = _next;
                while (token != null)
                {
                    if (token.SetSuspendedInternal(suspended))
                        result = true;
                    token = token._next;
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
                    SetSuspendedInternal(_provider.IsNotificationsSuspended);
                }

                _provider.OnBusyInfoChanged();
                return true;
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

            private bool SetSuspendedInternal(bool suspended)
            {
                return SetSuspended(ref _suspended, suspended);
            }

            private bool SetSuspendedExternal(bool suspended)
            {
                return SetSuspended(ref _suspendedExternal, suspended);
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
                    int size = 0;
                    var items = _listeners?.GetItems(out size);
                    if (items != null)
                    {
                        for (int i = 0; i < size; i++)
                            items[i].OnSuspendChanged(suspended);
                    }
                }
                return changed;
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