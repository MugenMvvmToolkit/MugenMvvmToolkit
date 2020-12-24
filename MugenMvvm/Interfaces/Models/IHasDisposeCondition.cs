using System;

namespace MugenMvvm.Interfaces.Models
{
    public interface IHasDisposeCondition : IDisposable
    {
        bool CanDispose { get; set; }
    }
}