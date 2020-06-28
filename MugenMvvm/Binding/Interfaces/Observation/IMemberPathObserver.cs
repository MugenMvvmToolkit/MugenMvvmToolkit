using System;
using System.Collections.Generic;
using MugenMvvm.Binding.Observation;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Interfaces.Observation
{
    public interface IMemberPathObserver : IWeakItem, IDisposable
    {
        object? Target { get; }

        IMemberPath Path { get; }

        void AddListener(IMemberPathObserverListener listener);

        void RemoveListener(IMemberPathObserverListener listener);

        ItemOrList<IMemberPathObserverListener, IReadOnlyList<IMemberPathObserverListener>> GetListeners();

        MemberPathMembers GetMembers(IReadOnlyMetadataContext? metadata = null);

        MemberPathLastMember GetLastMember(IReadOnlyMetadataContext? metadata = null);
    }
}