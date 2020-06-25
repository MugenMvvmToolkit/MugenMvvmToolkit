using System;

namespace MugenMvvm.Enums
{
    [Flags]
    public enum BusyMessageHandlerType : byte
    {
        None = 0,
        Handle = 1,
        NotifySubscribers = 1 << 1,
        HandleAndNotifySubscribers = Handle | NotifySubscribers
    }
}