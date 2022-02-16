using System;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Internal.Components
{
    public interface ILockerHandlerComponent<in T> : IComponent where T : class
    {
        void OnChanged(T owner, ILocker locker, IReadOnlyMetadataContext? metadata);

        bool TryWaitLockerUpdate(T owner, Action? onPendingUpdate, Action? onUpdated, IReadOnlyMetadataContext? metadata);
    }
}