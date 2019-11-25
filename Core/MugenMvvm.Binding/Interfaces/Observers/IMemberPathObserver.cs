using System;
using System.Collections.Generic;
using MugenMvvm.Binding.Observers;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Interfaces.Observers
{
    public interface IMemberPathObserver : IDisposable, IWeakItem
    {
        object? Target { get; }

        IMemberPath Path { get; }

        ItemOrList<IMemberPathObserverListener, IReadOnlyList<IMemberPathObserverListener>> Listeners { get; }

        void AddListener(IMemberPathObserverListener listener);

        void RemoveListener(IMemberPathObserverListener listener);

        MemberPathMembers GetMembers(IReadOnlyMetadataContext? metadata = null);

        MemberPathLastMember GetLastMember(IReadOnlyMetadataContext? metadata = null);
    }
}