using System;

namespace MugenMvvm.UnitTests.Models.Internal
{
    public class TestDisposable : IDisposable
    {
        public Action? Dispose { get; set; }

        void IDisposable.Dispose() => Dispose?.Invoke();
    }
}