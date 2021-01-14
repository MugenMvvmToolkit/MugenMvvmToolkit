using System;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;
using MugenMvvm.UnitTests.Models.Internal;

namespace MugenMvvm.UnitTests.Internal.Internal
{
    public class TestHasDisposeCallback : TestDisposable, IHasDisposeCallback
    {
        public Action<ActionToken>? RegisterDisposeToken { get; set; }

        void IHasDisposeCallback.RegisterDisposeToken(ActionToken token) => RegisterDisposeToken?.Invoke(token);
    }
}