using System;
using System.Threading;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Threading;

namespace MugenMvvm.UnitTest.TestInfrastructure
{
    public class TestThreadDispatcher : IThreadDispatcher
    {
        #region Constructors

        public TestThreadDispatcher()
        {
            ExecuteHandler = (handler, mode, arg3, ctx) => handler.Execute(arg3);
            ExecuteAction = (action, mode, arg3, ctx) => action(arg3);
        }

        #endregion

        #region Properties

        public bool IsOnMainThread { get; set; }

        public Action<IThreadDispatcherHandler, ThreadExecutionMode, object?, IReadOnlyMetadataContext?> ExecuteHandler { get; set; }

        public Action<Action<object?>, ThreadExecutionMode, object?, IReadOnlyMetadataContext?> ExecuteAction { get; set; }

        #endregion

        #region Implementation of interfaces

        void IThreadDispatcher.Execute(IThreadDispatcherHandler handler, ThreadExecutionMode executionMode, object? state, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            ExecuteHandler(handler, executionMode, state, metadata);
        }

        void IThreadDispatcher.Execute(Action<object?> action, ThreadExecutionMode executionMode, object? state, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            ExecuteAction(action, executionMode, state, metadata);
        }

        #endregion
    }
}