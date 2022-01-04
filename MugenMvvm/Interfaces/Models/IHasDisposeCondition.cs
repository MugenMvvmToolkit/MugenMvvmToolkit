using System;

namespace MugenMvvm.Interfaces.Models
{
    public interface IHasDisposeCondition : IDisposable//todo pooled, remove
    {
        bool IsDisposable { get; set; }
    }
}