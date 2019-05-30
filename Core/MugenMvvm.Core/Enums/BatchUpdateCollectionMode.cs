using System;

namespace MugenMvvm.Enums
{
    [Flags]
    public enum BatchUpdateCollectionMode
    {
        Listeners = 1,
        DecoratorListeners = 2,
        Both = Listeners | DecoratorListeners
    }
}