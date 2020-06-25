using System;
using MugenMvvm.Interfaces.Internal;

namespace MugenMvvm.UnitTest.Internal.Internal
{
    public class TestWeakReference : IWeakReference
    {
        #region Properties

        public bool IsAlive { get; set; }

        public object? Target { get; set; }

        public Action? Release { get; set; }

        #endregion

        #region Implementation of interfaces

        void IWeakReference.Release()
        {
            Release?.Invoke();
        }

        #endregion
    }
}