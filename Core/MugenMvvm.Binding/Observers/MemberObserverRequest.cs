using System.Reflection;
using System.Runtime.InteropServices;
using MugenMvvm.Binding.Interfaces.Members;

namespace MugenMvvm.Binding.Observers
{
    [StructLayout(LayoutKind.Auto)]
    public readonly struct MemberObserverRequest
    {
        #region Fields

        public readonly object?[] Arguments;
        public readonly MemberInfo? ReflectionMember;
        public readonly IMemberInfo? MemberInfo;
        public readonly string Path;

        #endregion

        #region Constructors

        public MemberObserverRequest(string path, MemberInfo? reflectionMember, object?[]? arguments, IMemberInfo? memberInfo = null)
        {
            Should.NotBeNull(path, nameof(path));
            MemberInfo = memberInfo;
            Path = path;
            ReflectionMember = reflectionMember;
            Arguments = arguments ?? Default.EmptyArray<object?>();
        }

        #endregion

        #region Properties

        public bool IsEmpty => ReferenceEquals(Path, null);

        #endregion
    }
}