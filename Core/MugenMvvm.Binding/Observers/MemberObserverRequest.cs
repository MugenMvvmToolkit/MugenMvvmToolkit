using System.Reflection;
using System.Runtime.InteropServices;

namespace MugenMvvm.Binding.Observers
{
    [StructLayout(LayoutKind.Auto)]
    public readonly struct MemberObserverRequest
    {
        #region Fields

        public readonly object?[] Arguments;
        public readonly MemberInfo Member;
        public readonly string Path;

        #endregion

        #region Constructors

        public MemberObserverRequest(string path, MemberInfo member, object?[] arguments)
        {
            Should.NotBeNull(member, nameof(member));
            Should.NotBeNull(arguments, nameof(arguments));
            Should.NotBeNull(path, nameof(path));
            Path = path;
            Member = member;
            Arguments = arguments;
        }

        #endregion

        #region Properties

        public bool IsEmpty => ReferenceEquals(Member, null);

        #endregion
    }
}