using System;
using System.Runtime.InteropServices;

namespace MugenMvvm.Binding.Compiling
{
    [StructLayout(LayoutKind.Auto)]
    public readonly struct ExpressionValue
    {
        #region Fields

        public readonly Type Type;
        public readonly object? Value;

        #endregion

        #region Constructors

        public ExpressionValue(Type type, object? value)
        {
            Type = type;
            Value = value;
        }

        #endregion

        #region Properties

        public bool IsEmpty => Type == null;

        #endregion
    }
}