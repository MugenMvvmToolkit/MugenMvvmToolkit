using System;
using System.Runtime.InteropServices;

namespace MugenMvvm.Binding.Compiling
{
    [StructLayout(LayoutKind.Auto)]
    public readonly struct ParameterValue
    {
        #region Fields

        public readonly Type Type;
        public readonly object? Value;

        #endregion

        #region Constructors

        public ParameterValue(Type type, object? value)
        {
            Should.NotBeNull(type, nameof(type));
            Type = type;
            Value = value;
        }

        #endregion

        #region Properties

        public bool IsEmpty => Type == null;

        #endregion
    }
}