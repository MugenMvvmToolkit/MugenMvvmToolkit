using System;

namespace MugenMvvm.Enums
{
    [Flags]
    public enum BatchUpdateCollectionMode : byte
    {
        Listeners = 1,
        DecoratorListeners = 2,
        Both = Listeners | DecoratorListeners
    }
}