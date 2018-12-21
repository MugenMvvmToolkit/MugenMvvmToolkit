using System;

namespace MugenMvvm.Interfaces.Models
{
    public interface ISuspendNotifications
    {
        bool IsNotificationsSuspended { get; }

        IDisposable SuspendNotifications();
    }
}