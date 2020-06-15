using System;

namespace MugenMvvm.Binding.Enums
{
    [Flags]
    public enum BindingMemberExpressionFlags : byte
    {
        StablePath = 1,
        Observable = 1 << 1,
        ObservableMethod = 1 << 2,
        Optional = 1 << 3,
        Target = 1 << 4
    }
}