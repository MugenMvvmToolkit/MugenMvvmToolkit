using System.Runtime.InteropServices;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Observation;

namespace MugenMvvm.Binding.Observation
{
    [StructLayout(LayoutKind.Auto)]
    public readonly struct MemberPathObserverRequest
    {
        #region Fields

        public readonly string? ObservableMethodName;
        public readonly IMemberPath Path;

        public readonly MemberFlags MemberFlags;
        public readonly bool HasStablePath;
        public readonly bool Observable;
        public readonly bool Optional;

        #endregion

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

        public bool IsEmpty => Path == null;

        #endregion
    }
}