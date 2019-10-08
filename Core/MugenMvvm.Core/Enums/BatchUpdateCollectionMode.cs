using System;

namespace MugenMvvm.Enums
{
    [Flags]
    public enum BatchUpdateCollectionMode : byte
    {
        Listeners = 1,
        DecoratorListeners = 1 << 1,
        Both = Listeners | DecoratorListeners
    }
}