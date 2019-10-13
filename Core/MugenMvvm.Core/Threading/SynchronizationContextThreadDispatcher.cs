using System;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Threading;

namespace MugenMvvm.Threading
{
    public sealed class SynchronizationContextThreadDispatcher : IThreadDispatcher
    {
        #region Fields

        private readonly SynchronizationContext _synchronizationContext;
        private int? _mainThreadId;

        #endregion

        #region Constructors

        public SynchronizationContextThreadDispatcher(SynchronizationContext synchronizationContext)
        {
            _synchronizationContext = synchronizationContext;
            synchronizationContext.Post(state => ((SynchronizationContextThreadDispatcher) state)._mainThreadId = Environment.CurrentManagedThreadId, this);
        }

        #endregion

        #region Implementation of interfaces

        public bool CanExecuteInline(ThreadExecutionMode executionMode)
        {
            Should.NotBeNull(executionMode, nameof(executionMode));
            return executionMode == ThreadExecutionMode.Current || executionMode == ThreadExecutionMode.Main && IsOnMainThread();
        }

        public void Execute(IThreadDispatcherHandler handler, ThreadExecutionMode executionMode, object? state, CancellationToken cancellationToken = default,
            IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(handler, nameof(handler));
            if (CanExecuteInline(executionMode))
            {
                if (!cancellationToken.IsCancellationRequested)
                    handler.Execute(state);
                return;
            }

            if (executionMode == ThreadExecutionMode.Main || executionMode == ThreadExecutionMode.MainAsync)
            {
                if (state == null)
                    _synchronizationContext.Post(o => ((IThreadDispatcherHandler) o).Execute(null), handler);
                else
                    _synchronizationContext.Post(handler.Execute, state);
                return;
            }

            if (executionMode == ThreadExecutionMode.Background)
            {
                //todo THREADPOOL
                if (state == null)
                    Task.Factory.StartNew(o => ((IThreadDispatcherHandler) o).Execute(null), handler, cancellationToken);
                else
                    Task.Factory.StartNew(handler.Execute, state, cancellationToken);
                return;
            }

            ExceptionManager.ThrowEnumOutOfRange(nameof(executionMode), executionMode);
        }

        public void Execute(Action<object?> action, ThreadExecutionMode executionMode, object? state, CancellationToken cancellationToken = default,
            IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(action, nameof(action));
            if (CanExecuteInline(executionMode))
            {
                if (!cancellationToken.IsCancellationRequested)
                    action.Invoke(state);
                return;
            }

            if (executionMode == ThreadExecutionMode.Main || executionMode == ThreadExecutionMode.MainAsync)
            {
                if (state == null)
                    _synchronizationContext.Post(o => ((Action<object?>) o).Invoke(null), action);
                else
                    _synchronizationContext.Post(new SendOrPostCallback(action), state);
                return;
            }

            if (executionMode == ThreadExecutionMode.Background)
            {
                //todo THREADPOOL
                if (state == null)
                    Task.Factory.StartNew(o => ((Action<object?>) o).Invoke(null), action, cancellationToken);
                else
                    Task.Factory.StartNew(action, state, cancellationToken);
                return;
            }

            ExceptionManager.ThrowEnumOutOfRange(nameof(executionMode), executionMode);
        }

        #endregion

        #region Methods

        private bool IsOnMainThread()
        {
            if (_mainThreadId == null)
                return SynchronizationContext.Current == _synchronizationContext;
            return _mainThreadId == Environment.CurrentManagedThreadId;
        }

        #endregion
    }
}