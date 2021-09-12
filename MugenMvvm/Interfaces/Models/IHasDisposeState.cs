using System;

namespace MugenMvvm.Interfaces.Models
{
    public interface IHasDisposeState : IDisposable
    {
        bool IsDisposed { get; }
    }
}