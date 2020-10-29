using System;

namespace MugenMvvm.Bindings.Interfaces.Observation
{
    public interface IMemberPathObserverListener
    {
        void OnPathMembersChanged(IMemberPathObserver observer);

        void OnLastMemberChanged(IMemberPathObserver observer);

        void OnError(IMemberPathObserver observer, Exception exception);
    }
}