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
        public readonly object Source;
        public readonly object? State;

        #endregion

        #region Constructors

        public MemberPathObserverRequest(object source, IMemberPath path, MemberFlags memberFlags, string? observableMethodName, bool hasStablePath, bool observable, bool optional, object? state = null)
        {
            Should.NotBeNull(source, nameof(source));
            Should.NotBeNull(path, nameof(path));
            Source = source;
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