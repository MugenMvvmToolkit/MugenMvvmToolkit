using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Internal.Components;
using MugenMvvm.Internal;

namespace MugenMvvm.Components
{
    public abstract class SynchronizableComponentOwnerBase<T> : ComponentOwnerBase<T>, ISynchronizable where T : class
    {
        private ILocker? _lastTakenLocker;
        private int _lockCount;
        private ILocker _locker;

        protected SynchronizableComponentOwnerBase(IComponentCollectionManager? componentCollectionManager) : base(componentCollectionManager)
        {
            _locker = PriorityLocker.GetLocker(this);
        }

        protected SynchronizableComponentOwnerBase(IComponentCollectionManager? componentCollectionManager, IComponentOwner<T>? owner) : base(componentCollectionManager, owner)
        {
            _locker = PriorityLocker.GetLocker(this);
        }

        ILocker ISynchronizable.Locker => _locker;

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

        public bool TryLock(int timeout, out ActionToken lockToken)
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
                    {
                        lockToken = default;
                        return false;
                    }

                    var currentLocker = _lastTakenLocker ?? _locker;
                    if (ReferenceEquals(currentLocker, locker))
                    {
                        _lastTakenLocker = locker;
                        ++_lockCount;
                        lockToken = ActionToken.FromDelegate((c, l) => ((SynchronizableComponentOwnerBase<T>) c!).Unlock((ILocker) l!), this, locker);
                        return true;
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

        private void Unlock(ILocker locker)
        {
            Should.BeValid(_lockCount > 0, nameof(_lockCount));
            if (--_lockCount == 0)
                _lastTakenLocker = null;
            locker.Exit();
        }

        void ISynchronizable.UpdateLocker(ILocker locker)
        {
            Should.NotBeNull(locker, nameof(locker));
            if (ReferenceEquals(locker, _locker))
                return;

            using var _ = Lock();
            if (locker.Priority > _locker.Priority)
            {
                _locker = locker;
                GetComponents<ILockerChangedListener<T>>().OnChanged((T) (object) this, locker, null);
            }
        }
    }
}