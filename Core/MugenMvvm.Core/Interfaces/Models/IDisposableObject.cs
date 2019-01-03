using System;

namespace MugenMvvm.Interfaces.Models
{
    public interface IDisposableObject<out T> : IDisposable
        where T : class
    {
        bool IsDisposed { get; }

        event Action<T, EventArgs> Disposed;
    }
}