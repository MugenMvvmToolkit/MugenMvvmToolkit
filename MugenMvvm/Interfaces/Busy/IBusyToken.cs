using System;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;

namespace MugenMvvm.Interfaces.Busy
{
    public interface IBusyToken : IDisposable, ISuspendable
    {
        IBusyManager Owner { get; }

        bool IsCompleted { get; }

        object? Message { get; }

        ActionToken RegisterCallback(IBusyTokenCallback callback);
    }
}