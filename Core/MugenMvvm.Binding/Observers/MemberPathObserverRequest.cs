using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Enums;

namespace MugenMvvm.Binding.Observers
{
    public readonly struct MemberPathObserverRequest
    {
        #region Fields

        public readonly bool HasStablePath;
        public readonly MemberFlags MemberFlags;
        public readonly bool Observable;
        public readonly string? ObservableMethodName;
        public readonly bool Optional;
        public readonly IMemberPath Path;
        public readonly object Target;
        public readonly object? State;

        #endregion

        #region Constructors

        public MemberPathObserverRequest(object target, IMemberPath path, MemberFlags memberFlags, string? observableMethodName, bool hasStablePath, bool observable, bool optional, object? state = null)
        {
            Should.NotBeNull(target, nameof(target));
            Should.NotBeNull(path, nameof(path));
            Target = target;
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