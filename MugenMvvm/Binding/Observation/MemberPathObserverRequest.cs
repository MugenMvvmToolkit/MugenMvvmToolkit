using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Observation;

namespace MugenMvvm.Binding.Observation
{
    public class MemberPathObserverRequest
    {
        #region Constructors

        public MemberPathObserverRequest(IMemberPath path, MemberFlags memberFlags, string? observableMethodName,
            bool hasStablePath, bool observable, bool optional)
        {
            Should.NotBeNull(path, nameof(path));
            Path = path;
            MemberFlags = memberFlags;
            HasStablePath = hasStablePath;
            Observable = observable;
            Optional = optional;
            ObservableMethodName = observable ? observableMethodName : null;
        }

        #endregion

        #region Properties

        public bool HasStablePath { get; protected set; }

        public MemberFlags MemberFlags { get; protected set; }

        public bool Observable { get; protected set; }

        public string? ObservableMethodName { get; protected set; }

        public bool Optional { get; protected set; }

        public IMemberPath Path { get; protected set; }

        #endregion
    }
}