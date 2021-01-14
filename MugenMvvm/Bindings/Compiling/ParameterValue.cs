using System;
using System.Runtime.InteropServices;

namespace MugenMvvm.Bindings.Compiling
{
    [StructLayout(LayoutKind.Auto)]
    public readonly struct ParameterValue
    {
        public readonly Type Type;
        public readonly object? Value;

        public ParameterValue(Type type, object? value)
        {
            Should.NotBeNull(type, nameof(type));
            Type = type;
            Value = value;
        }

        public bool IsEmpty => Type == null;
    }
}