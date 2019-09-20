using System;
using System.Threading;
using System.Threading.Tasks;
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

        public Func<ThreadExecutionMode, bool>? CanExecute { get; set; }

        public Action<IThreadDispatcherHandler, ThreadExecutionMode, object?, IReadOnlyMetadataContext?> ExecuteHandler { get; set; }

        public Action<Action<object?>, ThreadExecutionMode, object?, IReadOnlyMetadataContext?> ExecuteAction { get; set; }

        #endregion

        #region Implementation of interfaces

        bool IThreadDispatcher.CanExecuteInline(ThreadExecutionMode executionMode)
        {
            return CanExecute?.Invoke(executionMode) ?? true;
        }

        void IThreadDispatcher.Execute(IThreadDispatcherHandler handler, ThreadExecutionMode executionMode, object? state, CancellationToken cancellationToken,
            IReadOnlyMetadataContext? metadata)
        {
            ExecuteAsync(handler, executionMode, state, cancellationToken, metadata);
        }

        void IThreadDispatcher.Execute(Action<object?> action, ThreadExecutionMode executionMode, object? state, CancellationToken cancellationToken,
            IReadOnlyMetadataContext? metadata)
        {
            ExecuteAsync(action, executionMode, state, cancellationToken, metadata);
        }

        public Task ExecuteAsync(IThreadDispatcherHandler handler, ThreadExecutionMode executionMode, object? state, CancellationToken cancellationToken = default,
            IReadOnlyMetadataContext? metadata = null)
        {
            ExecuteHandler(handler, executionMode, state, metadata);
            return Default.CompletedTask;
        }

        public Task ExecuteAsync(Action<object?> action, ThreadExecutionMode executionMode, object? state, CancellationToken cancellationToken = default,
            IReadOnlyMetadataContext? metadata = null)
        {
            ExecuteAction(action, executionMode, state, metadata);
            return Default.CompletedTask;
        }

        #endregion
    }
}