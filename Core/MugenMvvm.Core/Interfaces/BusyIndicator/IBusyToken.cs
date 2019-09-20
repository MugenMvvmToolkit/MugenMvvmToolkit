using System;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Interfaces.BusyIndicator
{
    public interface IBusyToken : IDisposable, ISuspendable
    {
        bool IsCompleted { get; }

        object? Message { get; }

        void Register(IBusyTokenCallback callback);
    }
}