using System;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Enums;
using MugenMvvm;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Threading;

namespace MugenMvvm.Infrastructure.Threading
{
    public abstract class ThreadDispatcherBase : IThreadDispatcher
    {
        #region Implementation of interfaces

        public bool CanExecuteInline(ThreadExecutionMode executionMode)
        {
            Should.NotBeNull(executionMode, nameof(executionMode));
            return CanExecuteInlineInternal(executionMode);
        }

        public void Execute(IThreadDispatcherHandler handler, ThreadExecutionMode executionMode, object? state, CancellationToken cancellationToken = default,
            IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(handler, nameof(handler));
            Should.NotBeNull(executionMode, nameof(executionMode));
            ExecuteInternalAsync(handler, executionMode, state, false, cancellationToken, metadata);
        }

        public void Execute(Action<object?> action, ThreadExecutionMode executionMode, object? state, CancellationToken cancellationToken = default,
            IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(action, nameof(action));
            Should.NotBeNull(executionMode, nameof(executionMode));
            ExecuteInternalAsync(action, executionMode, state, false, cancellationToken, metadata);
        }

        public Task ExecuteAsync(IThreadDispatcherHandler handler, ThreadExecutionMode executionMode, object? state, CancellationToken cancellationToken = default,
            IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(handler, nameof(handler));
            Should.NotBeNull(executionMode, nameof(executionMode));
            return ExecuteInternalAsync(handler, executionMode, state, false, cancellationToken, metadata)!;
        }

        public Task ExecuteAsync(Action<object?> action, ThreadExecutionMode executionMode, object? state, CancellationToken cancellationToken = default,
            IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(action, nameof(action));
            Should.NotBeNull(executionMode, nameof(executionMode));
            return ExecuteInternalAsync(action, executionMode, state, false, cancellationToken, metadata)!;
        }

        #endregion

        #region Methods

        protected abstract Task? ExecuteOnMainThreadAsync(IThreadDispatcherHandler handler, object? state, bool includeTaskResult, CancellationToken cancellationToken,
            IReadOnlyMetadataContext? metadata);

        protected abstract Task? ExecuteOnMainThreadAsync(Action<object?> action, object? state, bool includeTaskResult, CancellationToken cancellationToken,
            IReadOnlyMetadataContext? metadata);

        protected abstract bool IsOnMainThread();

        protected virtual Task? ExecuteInternalAsync(IThreadDispatcherHandler handler, ThreadExecutionMode executionMode,
            object? state, bool includeTaskResult, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            if (executionMode == ThreadExecutionMode.Current || executionMode == ThreadExecutionMode.Main && IsOnMainThread())
            {
                handler.Execute(state);
                return Default.CompletedTask;
            }

            if (executionMode == ThreadExecutionMode.Main || executionMode == ThreadExecutionMode.MainAsync)
                return ExecuteOnMainThreadAsync(handler, state, includeTaskResult, cancellationToken, metadata);

            if (executionMode == ThreadExecutionMode.Background)
            {
                if (state == null)
                    return Task.Factory.StartNew(o => ((IThreadDispatcherHandler)o).Execute(null), handler, cancellationToken);
                return Task.Factory.StartNew(handler.ToExecuteDelegate, state, cancellationToken);
            }

            ExceptionManager.ThrowEnumOutOfRange(nameof(executionMode), executionMode);
            return null;
        }

        protected virtual Task? ExecuteInternalAsync(Action<object?> action, ThreadExecutionMode executionMode,
            object? state, bool includeTaskResult, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            if (executionMode == ThreadExecutionMode.Current || executionMode == ThreadExecutionMode.Main && IsOnMainThread())
            {
                action(state);
                return Default.CompletedTask;
            }

            if (executionMode == ThreadExecutionMode.Main || executionMode == ThreadExecutionMode.MainAsync)
                return ExecuteOnMainThreadAsync(action, state, includeTaskResult, cancellationToken, metadata);

            if (executionMode == ThreadExecutionMode.Background)
                return Task.Factory.StartNew(action, state, cancellationToken);

            ExceptionManager.ThrowEnumOutOfRange(nameof(executionMode), executionMode);
            return null;
        }

        protected virtual bool CanExecuteInlineInternal(ThreadExecutionMode executionMode)
        {
            if (executionMode == ThreadExecutionMode.Current)
                return true;
            if (executionMode == ThreadExecutionMode.MainAsync || executionMode == ThreadExecutionMode.Background)
                return false;
            return executionMode == ThreadExecutionMode.Main && IsOnMainThread();
        }

        #endregion
    }
}