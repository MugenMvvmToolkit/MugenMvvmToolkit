using System;
using MugenMvvm.Interfaces.Threading;

namespace MugenMvvm.UnitTest.Threading.Internal
{
    public class TestThreadDispatcherHandler : IThreadDispatcherHandler
    {
        #region Properties

        public Action<object>? Execute { get; set; }

        #endregion

        #region Implementation of interfaces

        void IThreadDispatcherHandler.Execute(object? state)
        {
            Execute?.Invoke(state!);
        }

        #endregion
    }
}