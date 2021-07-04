using System;
using System.Threading.Tasks;
using MugenMvvm.Collections;
using MugenMvvm.Components;
using MugenMvvm.Constants;
using MugenMvvm.Extensions;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Busy;
using MugenMvvm.Interfaces.Busy.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;

namespace MugenMvvm.Busy.Components
{
    public sealed class BusyTokenManager : MultiAttachableComponentBase<IBusyManager>, IBusyManagerComponent, IHasPriority
    {
        private BusyToken? _busyTail;

        public int Priority { get; init; } = BusyComponentPriority.BusyManager;

        private object Locker => this;

        public IBusyToken TryBeginBusy(IBusyManager busyManager, object? request, IReadOnlyMetadataContext? metadata)
        {
            var delay = 0;
            if (request is DelayBusyRequest delayBusyRequest)
            {
                delay = delayBusyRequest.Delay;
                request = delayBusyRequest.Message;
            }

            if (request is IBusyToken busyToken)
                return BeginBusy(new BusyToken(this, busyToken, busyManager), delay, metadata);
            return BeginBusy(new BusyToken(this, request, busyManager), delay, metadata);
        }

        public IBusyToken? TryGetToken<TState>(IBusyManager busyManager, Func<TState, IBusyToken, IReadOnlyMetadataContext?, bool> filter, TState state,
            IReadOnlyMetadataContext? metadata) =>
            _busyTail?.TryGetToken(filter, state, metadata);

        public ItemOrIReadOnlyList<IBusyToken> TryGetTokens(IBusyManager busyManager, IReadOnlyMetadataContext? metadata)
        {
            var busyToken = _busyTail;
            if (busyToken == null)
                return default;
            return busyToken.GetTokens();
        }

        protected override void OnAttached(IBusyManager owner, IReadOnlyMetadataContext? metadata) => OnBusyInfoChanged(metadata);

        protected override void OnDetached(IBusyManager owner, IReadOnlyMetadataContext? metadata) => OnBusyInfoChanged(metadata);

        private BusyToken BeginBusy(BusyToken busyToken, int millisecondsDelay, IReadOnlyMetadataContext? metadata)
        {
            if (millisecondsDelay > 0)
            {
                Task.Delay(millisecondsDelay)
                    .ContinueWith((_, state) =>
                    {
                        if (state is BusyToken token)
                            token.Owner.BeginBusy(token, 0, null);
                        else
                        {
                            var tuple = (Tuple<BusyToken, IReadOnlyMetadataContext>)state!;
                            tuple.Item1.Owner.BeginBusy(tuple.Item1, 0, tuple.Item2);
                        }
                    }, metadata == null ? busyToken : Tuple.Create(busyToken, metadata), TaskContinuationOptions.ExecuteSynchronously);
                return busyToken;
            }

            if (busyToken.SetSuspended(false) || !busyToken.IsCompleted)
            {
                foreach (var owner in Owners)
                    owner.GetComponents<IBusyManagerListener>(metadata).OnBeginBusy(owner, busyToken, metadata);
            }

            return busyToken;
        }

        private void OnBusyInfoChanged(IReadOnlyMetadataContext? metadata = null)
        {
            foreach (var owner in Owners)
                owner.GetComponents<IBusyManagerListener>(metadata).OnBusyStateChanged(owner, metadata);
        }

        private sealed class BusyToken : IBusyToken, IBusyTokenCallback
        {
            public readonly BusyTokenManager Owner;
            private readonly IBusyManager _owner;
            private object? _listeners;
            private BusyToken? _next;
            private BusyToken? _prev;
            private int _suspendCount;

            public BusyToken(BusyTokenManager owner, object? message, IBusyManager busyManager, IBusyToken? busyToken = null)
            {
                Message = message;
                Owner = owner;
                _owner = busyManager;
                ++_suspendCount;
                IsSuspended = true;
                lock (Locker)
                {
                    if (Owner._busyTail != null)
                    {
                        _prev = Owner._busyTail;
                        Owner._busyTail._next = this;
                    }

                    Owner._busyTail = this;

                    if (busyToken != null && busyToken.IsSuspended)
                        ++_suspendCount;
                }

                busyToken?.RegisterCallback(this);
            }

            public BusyToken(BusyTokenManager owner, IBusyToken token, IBusyManager busyManager)
                : this(owner, token.Message, busyManager, token)
            {
            }

            public bool IsCompleted => this == _listeners;

            public object? Message { get; }

            public bool IsSuspended { get; private set; }

            private object Locker => Owner.Locker;

            IBusyManager IBusyToken.Owner => _owner;

            public IBusyToken? TryGetToken<TState>(Func<TState, IBusyToken, IReadOnlyMetadataContext?, bool> filter, TState state, IReadOnlyMetadataContext? metadata)
            {
                Should.NotBeNull(filter, nameof(filter));
                lock (Locker)
                {
                    var token = Owner._busyTail;
                    while (token != null)
                    {
                        if (filter(state, token, metadata))
                            return token;
                        token = token._prev;
                    }
                }

                return null;
            }

            public ItemOrIReadOnlyList<IBusyToken> GetTokens()
            {
                var tokens = new ItemOrListEditor<IBusyToken>(2);
                lock (Locker)
                {
                    var token = Owner._busyTail;
                    while (token != null)
                    {
                        tokens.Add(token);
                        token = token._prev;
                    }
                }

                return tokens.ToItemOrList();
            }

            public bool SetSuspended(bool suspended)
            {
                lock (Locker)
                {
                    if (IsCompleted)
                        return false;

                    var value = (suspended ? ++_suspendCount : --_suspendCount) != 0;
                    if (IsSuspended == value)
                        return false;

                    IsSuspended = value;
                    foreach (var t in GetListeners())
                        t?.OnSuspendChanged(value);
                }

                Owner.OnBusyInfoChanged();
                return true;
            }

            public ActionToken RegisterCallback(IBusyTokenCallback callback)
            {
                Should.NotBeNull(callback, nameof(callback));
                if (!IsCompleted)
                {
                    lock (Locker)
                    {
                        if (!IsCompleted)
                        {
                            var editor = GetListenersEditor();
                            editor.Add(callback);
                            _listeners = editor.GetRawValue();

                            if (IsSuspended)
                                callback.OnSuspendChanged(true);
                            return ActionToken.FromDelegate((token, cal) => ((BusyToken)token!).RemoveCallback((IBusyTokenCallback)cal!), this, callback);
                        }
                    }
                }

                callback.OnCompleted(this);
                return default;
            }

            public void OnCompleted(IBusyToken token) => Dispose();

            public void OnSuspendChanged(bool suspended) => SetSuspended(suspended);

            public void Dispose()
            {
                ItemOrIReadOnlyList<IBusyTokenCallback> listeners;
                lock (Locker)
                {
                    listeners = GetListeners();
                    if (_prev != null)
                        _prev._next = _next;
                    if (_next == null)
                        Owner._busyTail = _prev;
                    else
                        _next._prev = _prev;
                    _listeners = this;
                }

                foreach (var t in listeners)
                    t.OnCompleted(this);

                Owner.OnBusyInfoChanged();
            }

            public ActionToken Suspend(object? state, IReadOnlyMetadataContext? metadata)
            {
                SetSuspended(true);
                return ActionToken.FromDelegate((t, _) => ((BusyToken)t!).SetSuspended(false), this);
            }

            private void RemoveCallback(IBusyTokenCallback callback)
            {
                lock (Locker)
                {
                    var list = GetListenersEditor();
                    list.Remove(callback);
                    _listeners = list.GetRawValue();
                }
            }

            private ItemOrListEditor<IBusyTokenCallback> GetListenersEditor()
            {
                if (IsCompleted)
                    return default;
                return ItemOrListEditor<IBusyTokenCallback>.FromRawValue(_listeners);
            }

            private ItemOrIReadOnlyList<IBusyTokenCallback> GetListeners()
            {
                if (IsCompleted)
                    return default;
                return ItemOrIReadOnlyList.FromRawValue<IBusyTokenCallback>(_listeners);
            }
        }
    }
}