using System;
using MugenMvvm.Bindings.Interfaces.Observation;

namespace MugenMvvm.UnitTests.Bindings.Observation.Internal
{
    public class TestMemberPathObserverListener : IMemberPathObserverListener
    {
        public int PathMembersChangedCount { get; set; }

        public int LastMemberChangedCount { get; set; }

        public int ErrorCount { get; set; }

        public Action<IMemberPathObserver>? OnPathMembersChanged { get; set; }

        public Action<IMemberPathObserver>? OnLastMemberChanged { get; set; }

        public Action<IMemberPathObserver, Exception>? OnError { get; set; }

        void IMemberPathObserverListener.OnPathMembersChanged(IMemberPathObserver observer)
        {
            ++PathMembersChangedCount;
            OnPathMembersChanged?.Invoke(observer);
        }

        void IMemberPathObserverListener.OnLastMemberChanged(IMemberPathObserver observer)
        {
            ++LastMemberChangedCount;
            OnLastMemberChanged?.Invoke(observer);
        }

        void IMemberPathObserverListener.OnError(IMemberPathObserver observer, Exception exception)
        {
            ++ErrorCount;
            OnError?.Invoke(observer, exception);
        }
    }
}