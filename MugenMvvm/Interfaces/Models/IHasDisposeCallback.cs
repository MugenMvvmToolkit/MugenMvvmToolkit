using System;
using MugenMvvm.Internal;

namespace MugenMvvm.Interfaces.Models
{
    public interface IHasDisposeCallback : IDisposable
    {
        void RegisterDisposeToken(ActionToken token);
    }
}