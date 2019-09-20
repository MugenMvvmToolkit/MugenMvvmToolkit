using System;

namespace MugenMvvm.Binding.Interfaces.Observers
{
    public interface IBindingPathObserverListener
    {
        void OnPathMembersChanged(IBindingPathObserver observer);

        void OnLastMemberChanged(IBindingPathObserver observer);

        void OnError(IBindingPathObserver observer, Exception exception);
    }
}