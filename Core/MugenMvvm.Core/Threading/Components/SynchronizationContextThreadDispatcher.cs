using System;
using System.Threading;
using MugenMvvm.Constants;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Threading;
using MugenMvvm.Interfaces.Threading.Components;

namespace MugenMvvm.Threading.Components
{
    public sealed class SynchronizationContextThreadDispatcher : IThreadDispatcherComponent, IHasPriority
    {
        #region Fields

        private readonly SynchronizationContext _synchronizationContext;
        private int? _mainThreadId;

        #endregion

        #region Constructors

        public SynchronizationContextThreadDispatcher(SynchronizationContext synchronizationContext, bool isOnMainThread = false)
        {
            Should.NotBeNull(synchronizationContext, nameof(synchronizationContext));
            _synchronizationContext = synchronizationContext;
            if (isOnMainThread)
                _mainThreadId = Thread.CurrentThread.ManagedThreadId;
            else
                synchronizationContext.Post(state => ((SynchronizationContextThreadDispatcher) state)._mainThreadId = Thread.CurrentThread.ManagedThreadId, this);
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = ThreadingComponentPriority.Dispatcher;

        #endregion

        #region Implementation of interfaces

        public bool CanExecuteInline(ThreadExecutionMode executionMode, IReadOnlyMetadataContext? metadata)
        {
            return executionMode == ThreadExecutionMode.Current || executionMode == ThreadExecutionMode.Main && IsOnMainThread();
        }

        public bool TryExecute<TState>(ThreadExecutionMode executionMode, object handler, TState state, IReadOnlyMetadataContext? metadata)
        {
            if (CanExecuteInline(executionMode, metadata))
                return ExecuteInline(handler, state);
            if (executionMode == ThreadExecutionMode.Main || executionMode == ThreadExecutionMode.MainAsync)
                return ExecuteMainThread(handler, state);
            if (executionMode == ThreadExecutionMode.Background)
                return ExecuteBackground(handler, state);
            return false;
        }

        #endregion

        #region Methods

        private static bool ExecuteBackground<TState>(object handler, TState state)
        {
            if (handler is WaitCallback waitCallback)
            {
                ThreadPool.QueueUserWorkItem(waitCallback, state);
                return true;
            }

            if (handler is Action)
            {
                ThreadPool.QueueUserWorkItem(o => ((Action) o).Invoke(), handler);
                return true;
            }

            if (handler is IThreadDispatcherHandler)
            {
                ThreadPool.QueueUserWorkItem(o => ((IThreadDispatcherHandler) o).Execute(), handler);
                return true;
            }

            if (handler is Action<TState> action)
            {
                ThreadPool.QueueUserWorkItem(action.Invoke, state);
                return true;
            }

            if (handler is IThreadDispatcherHandler<TState> handlerState)
            {
                ThreadPool.QueueUserWorkItem(handlerState.Execute, state);
                return true;
            }

            return false;
        }

        private bool ExecuteMainThread<TState>(object handler, TState state)
        {
            if (handler is SendOrPostCallback callback)
            {
                _synchronizationContext.Post(callback, state);
                return true;
            }

            if (handler is Action)
            {
                _synchronizationContext.Post(o => ((Action) o).Invoke(), handler);
                return true;
            }

            if (handler is IThreadDispatcherHandler)
            {
                _synchronizationContext.Post(o => ((IThreadDispatcherHandler) o).Execute(), handler);
                return true;
            }

            if (handler is Action<TState> action)
            {
                _synchronizationContext.Post(action.Invoke, state);
                return true;
            }

            if (handler is IThreadDispatcherHandler<TState> handlerState)
            {
                _synchronizationContext.Post(handlerState.Execute, state);
                return true;
            }

            return false;
        }

        private static bool ExecuteInline<TState>(object handler, TState state)
        {
            if (handler is Action action)
            {
                action.Invoke();
                return true;
            }

            if (handler is Action<TState> actionState)
            {
                actionState(state);
                return true;
            }

            if (handler is IThreadDispatcherHandler h)
            {
                h.Execute();
                return true;
            }

            if (handler is IThreadDispatcherHandler<TState> handlerState)
            {
                handlerState.Execute(state);
                return true;
            }

            return false;
        }

        private bool IsOnMainThread()
        {
            if (_mainThreadId == null)
                return SynchronizationContext.Current == _synchronizationContext;
            return _mainThreadId.Value == Environment.CurrentManagedThreadId;
        }

        #endregion
    }
}