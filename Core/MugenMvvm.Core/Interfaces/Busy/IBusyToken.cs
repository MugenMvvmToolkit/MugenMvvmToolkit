using System;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Interfaces.Busy
{
    public interface IBusyToken : IDisposable, ISuspendable
    {
        bool IsCompleted { get; }

        object? Message { get; }

        ActionToken Register(IBusyTokenCallback callback);
    }
}