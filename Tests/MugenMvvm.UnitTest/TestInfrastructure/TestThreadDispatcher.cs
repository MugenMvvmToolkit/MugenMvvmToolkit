using System;
using MugenMvvm.Interfaces;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Threading;
using MugenMvvm.Models;

namespace MugenMvvm.UnitTest.TestInfrastructure
{
    public class TestThreadDispatcher : IThreadDispatcher
    {
        #region Constructors

        public TestThreadDispatcher()
        {
            ExecuteHandler = (handler, mode, arg3) => handler.Execute(arg3);
            ExecuteAction = (action, mode, arg3) => action(arg3);
        }

        #endregion

        #region Properties

        public bool IsOnMainThread { get; set; }

        public Action<IThreadDispatcherHandler, ThreadExecutionMode, object?> ExecuteHandler { get; set; }

        public Action<Action<object?>, ThreadExecutionMode, object?> ExecuteAction { get; set; }

        #endregion

        #region Implementation of interfaces

        void IThreadDispatcher.Execute(IThreadDispatcherHandler handler, ThreadExecutionMode executionMode, object? state)
        {
            ExecuteHandler(handler, executionMode, state);
        }

        void IThreadDispatcher.Execute(Action<object?> action, ThreadExecutionMode executionMode, object? state)
        {
            ExecuteAction(action, executionMode, state);
        }

        #endregion
    }
}