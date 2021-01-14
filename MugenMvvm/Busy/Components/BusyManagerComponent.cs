﻿using System;
using System.Threading;
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
    public sealed class BusyManagerComponent : MultiAttachableComponentBase<IBusyManager>, IBusyManagerComponent, IHasPriority
    {
        private BusyToken? _busyTail;

        public int Priority => BusyComponentPriority.BusyManager;

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
                return BeginBusy(new BusyToken(this, busyToken), delay, metadata);
            return BeginBusy(new BusyToken(this, request), delay, metadata);
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
                            var tuple = (Tuple<BusyToken, IReadOnlyMetadataContext>) state!;
                            tuple.Item1.Owner.BeginBusy(tuple.Item1, 0, tuple.Item2);
                        }
                    }, metadata == null ? busyToken : (object) Tuple.Create(busyToken, metadata), TaskContinuationOptions.ExecuteSynchronously);
                return busyToken;
            }

            if (busyToken.Combine())
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
            public readonly BusyManagerComponent Owner;
            private object? _listeners;
            private BusyToken? _next;
            private BusyToken? _prev;
            private int _suspendCount;

            public BusyToken(BusyManagerComponent owner, object? message)
            {
                Message = message;
                Owner = owner;
            }

            public BusyToken(BusyManagerComponent owner, IBusyToken token)
            {
                token.RegisterCallback(this);
                Message = token.Message;
                Owner = owner;
            }

            public bool IsCompleted => this == _listeners;

            public object? Message { get; }

            public bool IsSuspended { get; private set; }

            private object Locker => Owner.Locker;

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
                var tokens = new ItemOrListEditor<IBusyToken>();
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

            public bool Combine()
            {
                if (IsCompleted)
                    return false;
                lock (Locker)
                {
                    if (IsCompleted)
                        return false;
                    if (Owner._busyTail != null)
                    {
                        _prev = Owner._busyTail;
                        Owner._busyTail._next = this;
                    }

                    Owner._busyTail = this;
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
                            return new ActionToken((token, cal) => ((BusyToken) token!).RemoveCallback((IBusyTokenCallback) cal!), this, callback);
                        }
                    }
                }

                callback.OnCompleted(this);
                return default;
            }

            public void OnCompleted(IBusyToken token) => Dispose();

            public void OnSuspendChanged(bool suspended)
            {
                if (suspended)
                    Suspend();
                else
                    OnEndSuspend();
            }

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

            public ActionToken Suspend(object? state = null, IReadOnlyMetadataContext? metadata = null)
            {
                if (Interlocked.Increment(ref _suspendCount) == 1)
                    SetSuspended(true);

                return new ActionToken((t, _) => ((BusyToken) t!).OnEndSuspend(), this);
            }

            private void OnEndSuspend()
            {
                if (Interlocked.Decrement(ref _suspendCount) == 0)
                    SetSuspended(false);
            }

            private void SetSuspended(bool suspended)
            {
                lock (Locker)
                {
                    if (IsCompleted)
                        return;
                    if (IsSuspended == suspended)
                        return;

                    IsSuspended = suspended;
                    foreach (var t in GetListeners())
                        t?.OnSuspendChanged(suspended);
                }

                Owner.OnBusyInfoChanged();
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