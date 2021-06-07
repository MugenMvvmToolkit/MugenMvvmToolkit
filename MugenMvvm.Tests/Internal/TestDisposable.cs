﻿using System;

namespace MugenMvvm.Tests.Internal
{
    public class TestDisposable : IDisposable
    {
        public Action? Dispose { get; set; }

        void IDisposable.Dispose() => Dispose?.Invoke();
    }
}