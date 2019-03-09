using System;

namespace MugenMvvm.Interfaces.Models
{
    public interface ISuspendable
    {
        bool IsSuspended { get; }

        IDisposable Suspend();
    }
}