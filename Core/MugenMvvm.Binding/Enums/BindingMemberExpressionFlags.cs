using System;

namespace MugenMvvm.Binding.Enums
{
    [Flags]
    public enum BindingMemberExpressionFlags : short
    {
        StablePath = 1,
        Observable = 1 << 1,
        ObservableMethod = 1 << 2,
        Optional = 1 << 3,
        TargetOnly = 1 << 4,
        SourceOnly = 1 << 5,
        ContextOnly = 1 << 6
    }
}