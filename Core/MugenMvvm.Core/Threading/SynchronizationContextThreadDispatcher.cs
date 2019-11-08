using System;
using System.Threading;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Internal;
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
            synchronizationContext.Post(state => ((SynchronizationContextThreadDispatcher)state)._mainThreadId = Thread.CurrentThread.ManagedThreadId, this);
        }

        #endregion

        #region Implementation of interfaces

        public bool CanExecuteInline(ThreadExecutionMode executionMode)
        {
            Should.NotBeNull(executionMode, nameof(executionMode));
            return executionMode == ThreadExecutionMode.Current || executionMode == ThreadExecutionMode.Main && IsOnMainThread();
        }

        public void Execute<TState>(ThreadExecutionMode executionMode, IThreadDispatcherHandler<TState> handler, TState state = default,
            CancellationToken cancellationToken = default, IReadOnlyMetadataContext? metadata = null)
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
                    _synchronizationContext.Post(o => ((IThreadDispatcherHandler<TState>)o).Execute(default!), handler);
                else
                {
                    if (handler is IValueHolder<Delegate> valueHolder)
                    {
                        if (!(valueHolder.Value is SendOrPostCallback sendOrPostCallback))
                        {
                            sendOrPostCallback = handler.Execute;
                            valueHolder.Value = sendOrPostCallback;
                        }

                        _synchronizationContext.Post(sendOrPostCallback, state);
                    }
                    else
                        _synchronizationContext.Post(handler.Execute, state);
                }

                return;
            }

            if (executionMode == ThreadExecutionMode.Background)
            {
                if (handler is IValueHolder<Delegate> valueHolder)
                {
                    if (!(valueHolder.Value is WaitCallback waitCallback))
                    {
                        waitCallback = handler.Execute;
                        valueHolder.Value = waitCallback;
                    }

                    ThreadPool.QueueUserWorkItem(waitCallback, state);
                }
                else
                    ThreadPool.QueueUserWorkItem(handler.Execute, state);

                return;
            }

            ExceptionManager.ThrowEnumOutOfRange(nameof(executionMode), executionMode);
        }

        public void Execute<TState>(ThreadExecutionMode executionMode, Action<TState> action, TState state = default,
            CancellationToken cancellationToken = default, IReadOnlyMetadataContext? metadata = null)
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
                    _synchronizationContext.Post(o => ((Action<TState>)o).Invoke(default!), action);
                else
                    _synchronizationContext.Post(action.Invoke, state);
                return;
            }

            if (executionMode == ThreadExecutionMode.Background)
            {
                if (state == null)
                    ThreadPool.QueueUserWorkItem(o => ((Action<TState>)o).Invoke(default!), action);
                else
                    ThreadPool.QueueUserWorkItem(action.Invoke, state);
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