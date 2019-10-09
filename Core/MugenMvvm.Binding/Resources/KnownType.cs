using System;
using System.Runtime.InteropServices;

namespace MugenMvvm.Binding.Resources
{
    [StructLayout(LayoutKind.Auto)]
    public readonly struct KnownType
    {
        #region Fields

        public readonly string Alias;
        public readonly Type Type;

        #endregion

        #region Constructors

        public KnownType(Type type, string alias)
        {
            Type = type;
            Alias = alias;
        }

        #endregion
    }
}