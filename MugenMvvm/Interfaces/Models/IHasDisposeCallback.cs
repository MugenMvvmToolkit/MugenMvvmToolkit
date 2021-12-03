using System;
using MugenMvvm.Internal;

namespace MugenMvvm.Interfaces.Models
{
    public interface IHasDisposeCallback : IDisposable
    {
        void RegisterDisposeToken(ActionToken token); //todo allow to remove dispose token?

#if !NET461
        void RegisterDisposeToken(IDisposable token) => RegisterDisposeToken(ActionToken.FromDisposable(token));
#endif
    }
}