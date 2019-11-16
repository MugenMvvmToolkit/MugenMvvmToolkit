using System;
using System.Runtime.InteropServices;

namespace MugenMvvm.Binding.Internal
{
    [StructLayout(LayoutKind.Auto)]
    internal readonly struct TypeStringKey
    {
        #region Fields

        public readonly string Name;
        public readonly Type Type;

        #endregion

        #region Constructors

        public TypeStringKey(Type type, string name)
        {
            Type = type;
            Name = name;
        }

        #endregion
    }
}