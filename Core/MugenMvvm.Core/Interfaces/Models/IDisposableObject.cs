using System;

namespace MugenMvvm.Interfaces.Models
{
    public interface IDisposableObject : IDisposable
    {
        bool IsDisposed { get; }

        event Action<IDisposableObject, EventArgs> Disposed;
    }
}