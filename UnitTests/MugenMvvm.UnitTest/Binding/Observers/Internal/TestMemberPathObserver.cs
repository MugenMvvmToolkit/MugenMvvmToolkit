using System;
using System.Collections.Generic;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Binding.Observers;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.UnitTest.Binding.Observers.Internal
{
    public class TestMemberPathObserver : IMemberPathObserver
    {
        #region Properties

        public bool IsAlive { get; set; }

        public object? Target { get; set; }

        public IMemberPath Path { get; set; } = null!;

        public Action? Dispose { get; set; }

        public Action<IMemberPathObserverListener>? AddListener { get; set; }

        public Action<IMemberPathObserverListener>? RemoveListener { get; set; }

        public Func<ItemOrList<IMemberPathObserverListener, IReadOnlyList<IMemberPathObserverListener>>>? GetListeners { get; set; }

        public GetMembersDelegate? GetMembers { get; set; }

        public GetLastMemberDelegate? GetLastMember { get; set; }

        #endregion

        #region Implementation of interfaces

        void IDisposable.Dispose()
        {
            Dispose?.Invoke();
        }

        void IMemberPathObserver.AddListener(IMemberPathObserverListener listener)
        {
            AddListener?.Invoke(listener);
        }

        void IMemberPathObserver.RemoveListener(IMemberPathObserverListener listener)
        {
            RemoveListener?.Invoke(listener);
        }

        ItemOrList<IMemberPathObserverListener, IReadOnlyList<IMemberPathObserverListener>> IMemberPathObserver.GetListeners()
        {
            return GetListeners?.Invoke() ?? default;
        }

        MemberPathMembers IMemberPathObserver.GetMembers(IReadOnlyMetadataContext? metadata)
        {
            if (GetMembers == null)
                return default;
            return GetMembers.Invoke(metadata);
        }

        MemberPathLastMember IMemberPathObserver.GetLastMember(IReadOnlyMetadataContext? metadata)
        {
            if (GetLastMember == null)
                return default;
            return GetLastMember.Invoke(metadata);
        }

        #endregion

        #region Nested types

        public delegate MemberPathLastMember GetLastMemberDelegate(IReadOnlyMetadataContext? metadata);

        public delegate MemberPathMembers GetMembersDelegate(IReadOnlyMetadataContext? metadata);

        #endregion
    }
}