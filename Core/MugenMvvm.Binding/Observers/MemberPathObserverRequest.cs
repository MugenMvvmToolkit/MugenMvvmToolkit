using System.Runtime.InteropServices;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Observers;

namespace MugenMvvm.Binding.Observers
{
    [StructLayout(LayoutKind.Auto)]
    public readonly struct MemberPathObserverRequest
    {
        #region Fields

        public readonly bool HasStablePath;
        public readonly BindingMemberFlags MemberFlags;
        public readonly bool Observable;
        public readonly string? ObservableMethodName;
        public readonly bool Optional;
        public readonly IMemberPath Path;
        public readonly object? State;

        #endregion

        #region Constructors

        public MemberPathObserverRequest(IMemberPath path, BindingMemberFlags memberFlags, string? observableMethodName,
            bool hasStablePath, bool observable, bool optional, object? state = null)
        {
            Should.NotBeNull(path, nameof(path));
            Path = path;
            MemberFlags = memberFlags;
            HasStablePath = hasStablePath;
            Observable = observable;
            Optional = optional;
            ObservableMethodName = observable ? observableMethodName : null;
            State = state;
        }

        #endregion
    }
}