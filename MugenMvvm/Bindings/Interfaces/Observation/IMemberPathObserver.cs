using MugenMvvm.Bindings.Observation;
using MugenMvvm.Collections;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Bindings.Interfaces.Observation
{
    public interface IMemberPathObserver : IWeakItem, IHasDisposeCondition
    {
        object? Target { get; }

        IMemberPath Path { get; }

        void AddListener(IMemberPathObserverListener listener);

        void RemoveListener(IMemberPathObserverListener listener);

        ItemOrIReadOnlyList<IMemberPathObserverListener> GetListeners();

        MemberPathMembers GetMembers(IReadOnlyMetadataContext? metadata = null);

        MemberPathLastMember GetLastMember(IReadOnlyMetadataContext? metadata = null);
    }
}