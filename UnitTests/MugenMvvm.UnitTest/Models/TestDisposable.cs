using System;

namespace MugenMvvm.UnitTest.Models
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