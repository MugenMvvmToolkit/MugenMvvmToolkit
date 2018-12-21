using System;

namespace MugenMvvm.Interfaces.BusyIndicator
{
    public interface IBusyToken : IDisposable
    {
        bool IsCompleted { get; }

        object? Message { get; }

        void Register(IBusyTokenCallback callback);
    }
}