using System;
using MugenMvvm.Interfaces.Threading;

namespace MugenMvvm.UnitTest.Threading
{
    public class TestThreadDispatcherHandler<T> : IThreadDispatcherHandler<T>
    {
        #region Properties

        public Action<object, Type>? Execute { get; set; }

        #endregion

        #region Implementation of interfaces

        void IThreadDispatcherHandler<T>.Execute(T state)
        {
            Execute?.Invoke(state!, typeof(T));
        }

        #endregion
    }
}