using System;

namespace MugenMvvm.Binding.Enums
{
    [Flags]
    public enum BindingMemberExpressionFlags : short
    {
        StablePath = 1,
        Observable = 1 << 1,
        Optional = 1 << 2,
        TargetOnly = 1 << 3,
        SourceOnly = 1 << 4,
        ContextOnly = 1 << 5
    }
}