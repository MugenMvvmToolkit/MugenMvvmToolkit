using System;

namespace MugenMvvm.Binding.Enums
{
    [Flags]
    public enum ArgumentFlags : byte
    {
        Metadata = 1 << 0,
        ParamArray = 1 << 1,
        EmptyParamArray = ParamArray | 1 << 2,
        Optional = 1 << 4
    }
}