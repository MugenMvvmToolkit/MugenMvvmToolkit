using System;
using MugenMvvm.Binding.Interfaces.Observation;

namespace MugenMvvm.UnitTests.Binding.Observation.Internal
{
    public class TestMemberPathObserverListener : IMemberPathObserverListener
    {
        #region Properties

        public int PathMembersChangedCount { get; set; }

        public int LastMemberChangedCount { get; set; }

        public int ErrorCount { get; set; }

        public Action<IMemberPathObserver>? OnPathMembersChanged { get; set; }

        public Action<IMemberPathObserver>? OnLastMemberChanged { get; set; }

        public Action<IMemberPathObserver, Exception>? OnError { get; set; }

        #endregion

        #region Implementation of interfaces

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

        #endregion
    }
}