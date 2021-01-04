using System;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;
using MugenMvvm.UnitTests.Models.Internal;

namespace MugenMvvm.UnitTests.Internal.Internal
{
    public class TestHasDisposeCallback : TestDisposable, IHasDisposeCallback
    {
        #region Properties

        public Action<ActionToken>? RegisterDisposeToken { get; set; }

        #endregion

        #region Implementation of interfaces

        void IHasDisposeCallback.RegisterDisposeToken(ActionToken token) => RegisterDisposeToken?.Invoke(token);

        #endregion
    }
}