using System;
using MugenMvvm.Internal;

namespace MugenMvvm.UnitTest
{
    public class TestHandler : ActionToken.IHandler
    {
        #region Properties

        public Action<object?, object?>? Invoke { get; set; }

        #endregion

        #region Implementation of interfaces

        void ActionToken.IHandler.Invoke(object? state1, object? state2)
        {
            Invoke?.Invoke(state1, state2);
        }

        #endregion
    }
}