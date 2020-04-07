using System;
using System.Runtime.InteropServices;
using MugenMvvm.Binding.Enums;

namespace MugenMvvm.Binding.Members
{
    [StructLayout(LayoutKind.Auto)]
    public readonly struct MemberManagerRequest
    {
        #region Fields

        public readonly MemberFlags Flags;
        public readonly MemberType MemberTypes;
        public readonly string Name;
        public readonly Type Type;

        #endregion

        #region Constructors

        public MemberManagerRequest(Type type, string name, MemberType memberTypes, MemberFlags flags)
        {
            Should.NotBeNull(type, nameof(type));
            Should.NotBeNull(name, nameof(name));
            Type = type;
            Name = name;
            MemberTypes = memberTypes;
            Flags = flags;
        }

        #endregion

        #region Properties

        public bool IsEmpty => Type == null;

        #endregion
    }
}