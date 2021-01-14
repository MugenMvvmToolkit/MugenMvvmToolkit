using System;
using System.Threading;
using MugenMvvm.Constants;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Threading;
using MugenMvvm.Interfaces.Threading.Components;

namespace MugenMvvm.Threading.Components
{
    public class SynchronizationContextThreadDispatcher : IThreadDispatcherComponent, IHasPriority
    {
        private readonly SynchronizationContext _synchronizationContext;
        private int? _mainThreadId;

        public SynchronizationContextThreadDispatcher(SynchronizationContext synchronizationContext, bool isOnMainThread = false)
        {
            Should.NotBeNull(synchronizationContext, nameof(synchronizationContext));
            _synchronizationContext = synchronizationContext;
            if (isOnMainThread)
                _mainThreadId = Thread.CurrentThread.ManagedThreadId;
            else
                synchronizationContext.Post(state => ((SynchronizationContextThreadDispatcher) state!)._mainThreadId = Thread.CurrentThread.ManagedThreadId, this);
        }

        public int Priority { get; set; } = ThreadingComponentPriority.Dispatcher;

        public bool CanExecuteInline(IThreadDispatcher threadDispatcher, ThreadExecutionMode executionMode, IReadOnlyMetadataContext? metadata)
            => executionMode == ThreadExecutionMode.Current || executionMode == ThreadExecutionMode.Main && IsOnMainThread() ||
               executionMode == ThreadExecutionMode.Background && !IsOnMainThread();

        public bool TryExecute(IThreadDispatcher threadDispatcher, ThreadExecutionMode executionMode, object handler, object? state, IReadOnlyMetadataContext? metadata)
        {
            if (CanExecuteInline(threadDispatcher, executionMode, metadata))
                return ExecuteInline(handler, state);
            if (executionMode == ThreadExecutionMode.Main || executionMode == ThreadExecutionMode.MainAsync)
                return ExecuteMainThread(handler, state);
            if (executionMode == ThreadExecutionMode.Background || executionMode == ThreadExecutionMode.BackgroundAsync)
                return ExecuteBackground(handler, state);
            return false;
        }

        protected virtual bool ExecuteBackground(object handler, object? state)
        {
            if (handler is IThreadDispatcherHandler h)
            {
                if (state == null)
                    ThreadPool.QueueUserWorkItem(o => ((IThreadDispatcherHandler) o!).Execute(null), handler);
                else
                    ThreadPool.QueueUserWorkItem(h.Execute, state);
                return true;
            }

            if (handler is WaitCallback waitCallback)
            {
                ThreadPool.QueueUserWorkItem(waitCallback, state);
                return true;
            }

            if (handler is Action)
            {
                ThreadPool.QueueUserWorkItem(o => ((Action) o!).Invoke(), handler);
                return true;
            }

            if (handler is Action<object?> action)
            {
                ThreadPool.QueueUserWorkItem(action.Invoke, state);
                return true;
            }

            return false;
        }

        protected virtual bool ExecuteMainThread(object handler, object? state)
        {
            if (handler is SendOrPostCallback callback)
            {
                _synchronizationContext.Post(callback, state);
                return true;
            }

            if (handler is Action)
            {
                _synchronizationContext.Post(o => ((Action) o!).Invoke(), handler);
                return true;
            }

            if (handler is IThreadDispatcherHandler h)
            {
                if (state == null)
                    _synchronizationContext.Post(o => ((IThreadDispatcherHandler) o!).Execute(null), handler);
                else
                    _synchronizationContext.Post(h.Execute, state);
                return true;
            }

            if (handler is Action<object?> action)
            {
                _synchronizationContext.Post(action.Invoke, state);
                return true;
            }

            return false;
        }

        protected virtual bool ExecuteInline(object handler, object? state)
        {
            if (handler is IThreadDispatcherHandler h)
            {
                h.Execute(state);
                return true;
            }

            if (handler is Action action)
            {
                action.Invoke();
                return true;
            }

            if (handler is Action<object?> actionState)
            {
                actionState(state);
                return true;
            }

            return false;
        }

        protected bool IsOnMainThread()
        {
            if (_mainThreadId == null)
                return SynchronizationContext.Current == _synchronizationContext;
            return _mainThreadId.Value == Environment.CurrentManagedThreadId;
        }
    }
}