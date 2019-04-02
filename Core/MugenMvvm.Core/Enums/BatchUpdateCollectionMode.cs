using System;

namespace MugenMvvm.Enums
{
    [Flags]
    public enum BatchUpdateCollectionMode
    {
        Listeners = 1,
        Decorators = 2,
        Both = Listeners | Decorators
    }
}