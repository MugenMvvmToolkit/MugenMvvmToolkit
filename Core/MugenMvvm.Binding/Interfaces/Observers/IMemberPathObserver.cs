using System;
using MugenMvvm.Binding.Observers;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Observers
{
    public interface IMemberPathObserver : IDisposable, IWeakItem
    {
        object? Target { get; }

        IMemberPath Path { get; }

        void AddListener(IMemberPathObserverListener listener);

        void RemoveListener(IMemberPathObserverListener listener);

        MemberPathMembers GetMembers(IReadOnlyMetadataContext? metadata = null);

        MemberPathLastMember GetLastMember(IReadOnlyMetadataContext? metadata = null);
    }
}