using System;
using MugenMvvm.Internal;

namespace MugenMvvm.UnitTests
{
    public class TestHandler : ActionToken.IHandler
    {
        public Action<object?, object?>? Invoke { get; set; }

        void ActionToken.IHandler.Invoke(object? state1, object? state2) => Invoke?.Invoke(state1, state2);
    }
}