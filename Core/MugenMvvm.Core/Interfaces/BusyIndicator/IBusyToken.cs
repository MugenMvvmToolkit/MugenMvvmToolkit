using System;

namespace MugenMvvm.Interfaces.BusyIndicator
{
    public interface IBusyToken : IDisposable//todo review types, callbacks
    {
        bool IsCompleted { get; }

        object? Message { get; }

        void Register(IBusyTokenCallback callback);
    }
}