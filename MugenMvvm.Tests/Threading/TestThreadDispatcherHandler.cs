﻿using System;
using MugenMvvm.Interfaces.Threading;

namespace MugenMvvm.Tests.Threading
{
    public class TestThreadDispatcherHandler : IThreadDispatcherHandler
    {
        public Action<object>? Execute { get; set; }

        void IThreadDispatcherHandler.Execute(object? state) => Execute?.Invoke(state!);
    }
}