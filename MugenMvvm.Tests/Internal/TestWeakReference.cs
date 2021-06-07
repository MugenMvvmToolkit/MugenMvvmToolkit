﻿using System;
using MugenMvvm.Interfaces.Internal;

namespace MugenMvvm.Tests.Internal
{
    public class TestWeakReference : IWeakReference
    {
        public Action? Release { get; set; }

        public bool IsAlive { get; set; }

        public object? Target { get; set; }

        void IWeakReference.Release() => Release?.Invoke();
    }
}