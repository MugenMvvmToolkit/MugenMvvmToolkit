using System;

namespace MugenMvvm.Interfaces.Models
{
    public interface IHasDisposeCondition : IDisposable
    {
        bool IsDisposable { get; set; }
    }
}