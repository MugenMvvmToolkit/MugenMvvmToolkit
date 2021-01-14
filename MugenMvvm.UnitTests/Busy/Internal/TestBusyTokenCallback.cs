using System;
using MugenMvvm.Interfaces.Busy;

namespace MugenMvvm.UnitTests.Busy.Internal
{
    public class TestBusyTokenCallback : IBusyTokenCallback
    {
        public Action<IBusyToken>? OnCompleted { get; set; }

        public Action<bool>? OnSuspendChanged { get; set; }

        void IBusyTokenCallback.OnCompleted(IBusyToken token) => OnCompleted?.Invoke(token);

        void IBusyTokenCallback.OnSuspendChanged(bool suspended) => OnSuspendChanged?.Invoke(suspended);
    }
}