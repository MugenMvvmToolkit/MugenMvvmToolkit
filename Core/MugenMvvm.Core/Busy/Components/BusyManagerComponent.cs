using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Constants;
using MugenMvvm.Extensions;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Busy;
using MugenMvvm.Interfaces.Busy.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;

namespace MugenMvvm.Busy.Components
{
    public sealed class BusyManagerComponent : IBusyManagerComponent, IAttachableComponent, IDetachableComponent, IHasPriority
    {
        #region Fields

        private BusyToken? _busyTail;
        private IBusyManager? _owner;
        private int _suspendCount;

        #endregion

        #region Properties

        public bool IsSuspended => _suspendCount != 0;

        public int Priority => BusyComponentConstants.BusyManager;

        private object Locker => this;

        #endregion

        #region Implementation of interfaces

        bool IAttachableComponent.OnAttaching(object owner, IReadOnlyMetadataContext? metadata)
        {
            return true;
        }

        void IAttachableComponent.OnAttached(object owner, IReadOnlyMetadataContext? metadata)
        {
            _owner = owner as IBusyManager;
            if (_owner != null)
                OnBusyInfoChanged(metadata: metadata);
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
            return new ActionToken((o, _) => ((BusyManagerComponent)o!).EndSuspendNotifications(), this);
        }

        public IBusyToken? TryBeginBusy<TRequest>(in TRequest request, IReadOnlyMetadataContext? metadata)
        {
            if (Default.IsValueType<TRequest>())
            {
                var busyRequest = MugenExtensions.CastGeneric<TRequest, BeginBusyRequest>(request);
                if (busyRequest.ParentToken != null)
                    return Begin(busyRequest.ParentToken, busyRequest.MillisecondsDelay, metadata);
                return Begin(busyRequest.Message, busyRequest.MillisecondsDelay, metadata);
            }

            if (request is IBusyToken busyToken)
                return Begin(busyToken, 0, metadata);
            return Begin(request, 0, metadata);
        }

        public IBusyToken? TryGetToken<TState>(in TState state, Func<TState, IBusyToken, IReadOnlyMetadataContext?, bool> filter, IReadOnlyMetadataContext? metadata)
        {
            return _busyTail?.TryGetToken(filter, state, metadata);
        }

        public IReadOnlyList<IBusyToken>? TryGetTokens(IReadOnlyMetadataContext? metadata)
        {
            return _busyTail?.GetTokens();
        }

        bool IDetachableComponent.OnDetaching(object owner, IReadOnlyMetadataContext? metadata)
        {
            return true;
        }

        void IDetachableComponent.OnDetached(object owner, IReadOnlyMetadataContext? metadata)
        {
            if (ReferenceEquals(_owner, owner))
            {
                OnBusyInfoChanged(metadata: metadata);
                _owner = null;
            }
        }

        #endregion

        #region Methods

        private IBusyToken Begin(IBusyToken parentToken, int millisecondsDelay, IReadOnlyMetadataContext? metadata)
        {
            var busyToken = new BusyToken(this, parentToken);
            BeginBusyInternal(busyToken, millisecondsDelay, metadata);
            return busyToken;
        }

        private IBusyToken Begin(object? message, int millisecondsDelay, IReadOnlyMetadataContext? metadata)
        {
            var busyToken = new BusyToken(this, message);
            BeginBusyInternal(busyToken, millisecondsDelay, metadata);
            return busyToken;
        }

        private void BeginBusyInternal(BusyToken busyToken, int millisecondsDelay, IReadOnlyMetadataContext? metadata)
        {
            if (millisecondsDelay != 0)
            {
                Task.Delay(millisecondsDelay)
                    .ContinueWith((_, state) =>
                    {
                        if (state is BusyToken token)
                            token.Owner.BeginBusyInternal(token, 0, null);
                        else
                        {
                            var tuple = (Tuple<BusyToken, IReadOnlyMetadataContext>)state!;
                            tuple.Item1.Owner.BeginBusyInternal(tuple.Item1, 0, tuple.Item2);
                        }
                    }, metadata == null ? busyToken : (object)Tuple.Create(busyToken, metadata), TaskContinuationOptions.ExecuteSynchronously);
                return;
            }

            if (busyToken.Combine())
                _owner?.GetComponents<IBusyManagerListener>().OnBeginBusy(_owner, busyToken, metadata);
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

        private void OnBusyInfoChanged(bool ignoreSuspend = false, IReadOnlyMetadataContext? metadata = null)
        {
            if (ignoreSuspend || !IsSuspended)
                _owner?.GetComponents<IBusyManagerListener>().OnBusyChanged(_owner, metadata);
        }

        #endregion

        #region Nested types

        private sealed class BusyToken : IBusyToken, IBusyTokenCallback
        {
            #region Fields

            public readonly BusyManagerComponent Owner;
            private object? _listeners;
            private BusyToken? _next;
            private BusyToken? _prev;
            private bool _suspended;
            private bool _suspendedExternal;
            private int _suspendExternalCount;

            #endregion

            #region Constructors

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

            #endregion

            #region Properties

            public bool IsCompleted => ReferenceEquals(this, _listeners);

            public object? Message { get; }

            public bool IsSuspended => _suspended || _suspendedExternal;

            private object Locker => Owner.Locker;

            #endregion

            #region Implementation of interfaces

            public ActionToken RegisterCallback(IBusyTokenCallback callback)
            {
                Should.NotBeNull(callback, nameof(callback));
                if (!IsCompleted)
                {
                    lock (Locker)
                    {
                        if (!IsCompleted)
                        {
                            var list = GetListeners();
                            list.Add(callback);
                            _listeners = list.GetRawValue();

                            if (IsSuspended)
                                callback.OnSuspendChanged(true);
                            return new ActionToken((token, cal) => ((BusyToken)token!).RemoveCallback((IBusyTokenCallback)cal!), this, callback);
                        }
                    }
                }

                callback.OnCompleted(this);
                return default;
            }

            public ActionToken Suspend()
            {
                return SuspendExternal(true);
            }

            public void Dispose()
            {
                ItemOrList<IBusyTokenCallback, List<IBusyTokenCallback>> listeners;
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

                for (var i = 0; i < listeners.Count(); i++)
                    listeners.Get(i).OnCompleted(this);
                Owner.OnBusyInfoChanged();
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

            public IBusyToken? TryGetToken<TState>(Func<TState, IBusyToken, IReadOnlyMetadataContext?, bool> filter, in TState state, IReadOnlyMetadataContext? metadata)
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

            public IReadOnlyList<IBusyToken>? GetTokens()
            {
                LazyList<IBusyToken> tokens = default;
                lock (Locker)
                {
                    var token = Owner._busyTail;
                    while (token != null)
                    {
                        tokens.Get().Insert(0, token);
                        token = token._prev;
                    }
                }

                return tokens.List;
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
                    if (Owner._busyTail != null)
                    {
                        _prev = Owner._busyTail;
                        Owner._busyTail._next = this;
                    }

                    Owner._busyTail = this;
                    SetSuspendedInternal(Owner.IsSuspended);
                }

                Owner.OnBusyInfoChanged();
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
                bool notify;
                lock (Locker)
                {
                    notify = SetSuspended(ref _suspendedExternal, suspended);
                }

                if (notify)
                    Owner.OnBusyInfoChanged();
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
                    var listeners = GetListeners();
                    for (var i = 0; i < listeners.Count(); i++)
                        listeners.Get(i)?.OnSuspendChanged(suspended);
                }

                return changed;
            }

            private void RemoveCallback(IBusyTokenCallback callback)
            {
                lock (Locker)
                {
                    var list = GetListeners();
                    list.Remove(callback);
                    _listeners = list.GetRawValue();
                }
            }

            private ItemOrList<IBusyTokenCallback, List<IBusyTokenCallback>> GetListeners()
            {
                if (IsCompleted)
                    return default;
                return ItemOrList<IBusyTokenCallback, List<IBusyTokenCallback>>.FromRawValue(_listeners);
            }

            #endregion
        }

        #endregion
    }
}