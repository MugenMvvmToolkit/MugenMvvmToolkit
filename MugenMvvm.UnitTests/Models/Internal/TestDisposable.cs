using System;

namespace MugenMvvm.UnitTests.Models.Internal
{
    public class TestDisposable : IDisposable
    {
        #region Properties

        public Action? Dispose { get; set; }

        #endregion

        #region Implementation of interfaces

        void IDisposable.Dispose() => Dispose?.Invoke();

        #endregion
    }
}