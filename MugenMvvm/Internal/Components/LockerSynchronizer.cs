using System;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Internal.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Internal.Components
{
    internal sealed class LockerSynchronizer<T> : ILockerHandlerComponent<T>, ActionToken.IHandler where T : class
    {
        private readonly ISynchronizable _x1;
        private readonly ISynchronizable _x2;

        public LockerSynchronizer(ISynchronizable x1, ISynchronizable x2, IReadOnlyMetadataContext? metadata)
        {
            _x1 = x1;
            _x2 = x2;
            x1.UpdateLocker(x2.Locker, metadata);
            x2.UpdateLocker(x1.Locker, metadata);
        }

        public bool IsSynchronized(ISynchronizable x1, ISynchronizable x2)
        {
            if (_x1 == x1)
                return _x2 == x2;
            return _x2 == x1 && _x1 == x2;
        }

        public void Invoke(object? state1, object? state2)
        {
            ((IComponentOwner) state1!).Components.Remove(this);
            ((IComponentOwner) state2!).Components.Remove(this);
        }

        public void OnChanged(T owner, ILocker locker, IReadOnlyMetadataContext? metadata)
        {
            if (owner == _x1)
                _x2.UpdateLocker(locker, metadata);
            else
                _x1.UpdateLocker(locker, metadata);
        }

        public bool TryWaitLockerUpdate(T owner, Action? onPendingUpdate, Action? onUpdated, IReadOnlyMetadataContext? metadata)
        {
            if (owner == _x1)
                return _x2.WaitLockerUpdate(true, onPendingUpdate, onUpdated, metadata);
            return _x1.WaitLockerUpdate(true, onPendingUpdate, onUpdated, metadata);
        }
    }
}