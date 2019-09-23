using System;
using MugenMvvm.Binding.Observers;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Observers
{
    public interface IMemberPathObserver : IDisposable
    {
        bool IsAlive { get; }

        IMemberPath Path { get; }

        object? Source { get; }

        void AddListener(IMemberPathObserverListener listener);

        void RemoveListener(IMemberPathObserverListener listener);

        MemberPathMembers GetMembers(IReadOnlyMetadataContext? metadata = null);

        MemberPathLastMember GetLastMember(IReadOnlyMetadataContext? metadata = null);
    }
}