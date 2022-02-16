using System;
using System.Threading;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Internal.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;
using MugenMvvm.Metadata;

namespace MugenMvvm.Components
{
    public abstract class SynchronizableComponentOwnerBase<T> : ComponentOwnerBase<T>, ISynchronizable where T : class
    {
        private Action? _onUpdated;
        private bool _isNestedCall;
        private ILocker? _lastTakenLocker;
        private int _lockCount;
        private ILocker _locker;
        private int? _waitLockerUpdateTimeout;

        protected SynchronizableComponentOwnerBase(IComponentCollectionManager? componentCollectionManager) : base(componentCollectionManager)
        {
            _locker = PriorityLocker.GetLocker(this);
        }

        protected SynchronizableComponentOwnerBase(IComponentCollectionManager? componentCollectionManager, IComponentOwner<T>? owner) : base(componentCollectionManager, owner)
        {
            _locker = PriorityLocker.GetLocker(this);
        }

        public int WaitLockerUpdateTimeout
        {
            get => _waitLockerUpdateTimeout.GetValueOrDefault(ApplicationMetadata.WaitLockerUpdateTimeout);
            set => _waitLockerUpdateTimeout = value;
        }

        // ReSharper disable once ConvertToAutoPropertyWithPrivateSetter
        public ILocker Locker => _locker;

        public ActionToken Lock()
        {
            var lockTaken = false;
            ILocker? locker = null;
            try
            {
                while (true)
                {
                    locker = _lastTakenLocker ?? _locker;
                    locker.Enter(ref lockTaken);

                    var currentLocker = _lastTakenLocker ?? _locker;
                    if (ReferenceEquals(currentLocker, locker))
                    {
                        if (lockTaken)
                        {
                            _lastTakenLocker = locker;
                            ++_lockCount;
                            return ActionToken.FromDelegate((c, l) => ((SynchronizableComponentOwnerBase<T>) c!).Unlock((ILocker) l!), this, locker);
                        }

                        return default;
                    }

                    if (lockTaken)
                    {
                        lockTaken = false;
                        locker.Exit();
                    }
                }
            }
            catch
            {
                if (lockTaken && locker != null)
                    locker.Exit();
                throw;
            }
        }

        public ActionToken TryLock(int timeout)
        {
            var lockTaken = false;
            ILocker? locker = null;
            try
            {
                while (true)
                {
                    locker = _lastTakenLocker ?? _locker;
                    locker.TryEnter(timeout, ref lockTaken);
                    if (!lockTaken)
                        return default;

                    var currentLocker = _lastTakenLocker ?? _locker;
                    if (ReferenceEquals(currentLocker, locker))
                    {
                        _lastTakenLocker = locker;
                        ++_lockCount;
                        return ActionToken.FromDelegate((c, l) => ((SynchronizableComponentOwnerBase<T>) c!).Unlock((ILocker) l!), this, locker);
                    }

                    lockTaken = false;
                    locker.Exit();
                }
            }
            catch
            {
                if (lockTaken && locker != null)
                    locker.Exit();
                throw;
            }
        }

        public bool WaitLockerUpdate(bool includeNested, Action? onPendingUpdate, Action? onUpdated, IReadOnlyMetadataContext? metadata)
        {
            if (!includeNested && (_lastTakenLocker == null || ReferenceEquals(_locker, _lastTakenLocker)))
                return false;

            using var token = TryLock(WaitLockerUpdateTimeout);
            if (!token.IsEmpty)
                return WaitLockerUpdateInternal(includeNested, onPendingUpdate, onUpdated, metadata);

            onPendingUpdate?.Invoke();
#if NET5_0
            ThreadPool.QueueUserWorkItem(static state =>
            {
                ActionToken locker = default;
                try
                {
                    locker = state.Item1.Lock();
                    state.Item1.WaitLockerUpdateInternal(state.includeNested, state.onPendingUpdate, state.onUpdated, state.metadata);
                }
                finally
                {
                    locker.Dispose();
                    state.onUpdated?.Invoke();
                }
            }, (this, includeNested, onPendingUpdate, onUpdated, metadata), true);
#else
            ThreadPool.QueueUserWorkItem(static s =>
            {
                var state = (Tuple<SynchronizableComponentOwnerBase<T>, bool, Action?, Action?, IReadOnlyMetadataContext?>) s;
                ActionToken locker = default;
                try
                {
                    locker = state.Item1.Lock();
                    state.Item1.WaitLockerUpdateInternal(state.Item2, state.Item3, state.Item4, state.Item5);
                }
                finally
                {
                    locker.Dispose();
                    state.Item4?.Invoke();
                }
            }, Tuple.Create(this, includeNested, onPendingUpdate, onUpdated, metadata));
#endif
            return true;
        }

        public void UpdateLocker(ILocker locker, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(locker, nameof(locker));
            if (ReferenceEquals(locker, _locker))
                return;

            using var _ = Lock();
            if (locker.Priority > _locker.Priority)
            {
                _locker = locker;
                GetComponents<ILockerHandlerComponent<T>>().OnChanged((T) (object) this, locker, null);
            }
        }

        private bool WaitLockerUpdateInternal(bool includeNested, Action? onPendingUpdate, Action? onUpdated, IReadOnlyMetadataContext? metadata)
        {
            if (_isNestedCall)
                return false;

            bool result;
            if (includeNested)
            {
                try
                {
                    _isNestedCall = true;
                    result = GetComponents<ILockerHandlerComponent<T>>().TryWaitLockerUpdate((T) (object) this, onPendingUpdate, onUpdated, metadata);
                }
                finally
                {
                    _isNestedCall = false;
                }
            }
            else
                result = false;

            if (ReferenceEquals(_lastTakenLocker, _locker))
                return result;

            onPendingUpdate?.Invoke();
            _onUpdated += onUpdated;
            return true;
        }

        private void Unlock(ILocker locker)
        {
            Should.BeValid(_lockCount > 0, nameof(_lockCount));
            Action? onUpdated = null;
            if (--_lockCount == 0)
            {
                if (!ReferenceEquals(_locker, _lastTakenLocker))
                {
                    onUpdated = _onUpdated;
                    _onUpdated = null;
                }

                _lastTakenLocker = null;
            }

            locker.Exit();
            onUpdated?.Invoke();
        }
    }
}