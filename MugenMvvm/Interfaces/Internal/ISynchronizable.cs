using System;
using JetBrains.Annotations;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Interfaces.Internal
{
    public interface ISynchronizable
    {
        ILocker Locker { get; }

        void UpdateLocker(ILocker locker, IReadOnlyMetadataContext? metadata);

        bool WaitLockerUpdate(bool includeNested, Action? onPendingUpdate, Action? onUpdated, IReadOnlyMetadataContext? metadata);

        [MustUseReturnValue]
        ActionToken Lock();

        [MustUseReturnValue]
        ActionToken TryLock(int timeout);
    }
}