using System;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;

namespace MugenMvvm.Tests.Internal
{
    public class TestHasDisposeCallback : TestDisposable, IHasDisposeCallback
    {
        public Action<ActionToken>? RegisterDisposeToken { get; set; }

        void IHasDisposeCallback.RegisterDisposeToken(ActionToken token) => RegisterDisposeToken?.Invoke(token);
    }
}