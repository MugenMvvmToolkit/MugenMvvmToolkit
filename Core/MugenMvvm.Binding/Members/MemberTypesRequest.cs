using System;
using System.Runtime.InteropServices;

namespace MugenMvvm.Binding.Members
{
    [StructLayout(LayoutKind.Auto)]
    public readonly struct MemberTypesRequest
    {
        #region Fields

        public readonly string Name;
        public readonly Type[] Types;

        #endregion

        #region Constructors

        public MemberTypesRequest(string name, Type[] types)
        {
            Should.NotBeNull(name, nameof(name));
            Should.NotBeNull(types, nameof(types));
            Name = name;
            Types = types;
        }

        #endregion

        #region Properties

        public bool IsEmpty => Name == null;

        #endregion
    }
}