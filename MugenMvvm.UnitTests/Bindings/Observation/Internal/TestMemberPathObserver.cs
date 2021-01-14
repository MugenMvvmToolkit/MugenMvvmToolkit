using System;
using MugenMvvm.Bindings.Interfaces.Observation;
using MugenMvvm.Bindings.Observation;
using MugenMvvm.Collections;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.UnitTests.Bindings.Observation.Internal
{
    public class TestMemberPathObserver : IMemberPathObserver
    {
        public delegate MemberPathLastMember GetLastMemberDelegate(IReadOnlyMetadataContext? metadata);

        public delegate MemberPathMembers GetMembersDelegate(IReadOnlyMetadataContext? metadata);

        public Action? Dispose { get; set; }

        public Action<IMemberPathObserverListener>? AddListener { get; set; }

        public Action<IMemberPathObserverListener>? RemoveListener { get; set; }

        public Func<ItemOrIReadOnlyList<IMemberPathObserverListener>>? GetListeners { get; set; }

        public GetMembersDelegate? GetMembers { get; set; }

        public GetLastMemberDelegate? GetLastMember { get; set; }

        public bool IsDisposable { get; set; } = true;

        public object? Target { get; set; }

        public IMemberPath Path { get; set; } = null!;

        public bool IsAlive { get; set; }

        void IDisposable.Dispose() => Dispose?.Invoke();

        void IMemberPathObserver.AddListener(IMemberPathObserverListener listener) => AddListener?.Invoke(listener);

        void IMemberPathObserver.RemoveListener(IMemberPathObserverListener listener) => RemoveListener?.Invoke(listener);

        ItemOrIReadOnlyList<IMemberPathObserverListener> IMemberPathObserver.GetListeners() => GetListeners?.Invoke() ?? default;

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
    }
}