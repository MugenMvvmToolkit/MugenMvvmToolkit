using System;
using System.Threading;
using MugenMvvm.Constants;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Threading;
using MugenMvvm.Interfaces.Threading.Components;

namespace MugenMvvm.Threading.Components
{
    public sealed class SynchronizationContextThreadDispatcherComponent : IThreadDispatcherComponent, IHasPriority
    {
        #region Fields

        private readonly SynchronizationContext _synchronizationContext;
        private int? _mainThreadId;

        #endregion

        #region Constructors

        public SynchronizationContextThreadDispatcherComponent(SynchronizationContext synchronizationContext, bool isOnMainThread = false)
        {
            Should.NotBeNull(synchronizationContext, nameof(synchronizationContext));
            _synchronizationContext = synchronizationContext;
            if (isOnMainThread)
                _mainThreadId = Thread.CurrentThread.ManagedThreadId;
            else
                synchronizationContext.Post(state => ((SynchronizationContextThreadDispatcherComponent) state)._mainThreadId = Thread.CurrentThread.ManagedThreadId, this);
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = ThreadingComponentPriority.Dispatcher;

        #endregion

        #region Implementation of interfaces

        public bool CanExecuteInline(ThreadExecutionMode executionMode)
        {
            return executionMode == ThreadExecutionMode.Current || executionMode == ThreadExecutionMode.Main && IsOnMainThread();
        }

        public bool TryExecute<TState>(ThreadExecutionMode executionMode, IThreadDispatcherHandler<TState> handler, TState state, IReadOnlyMetadataContext? metadata)
        {
            if (CanExecuteInline(executionMode))
            {
                handler.Execute(state);
                return true;
            }

            if (executionMode == ThreadExecutionMode.Main || executionMode == ThreadExecutionMode.MainAsync)
            {
                if (state == null)
                    _synchronizationContext.Post(o => ((IThreadDispatcherHandler<TState>) o).Execute(default!), handler);
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

                return true;
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

                return true;
            }

            return false;
        }

        public bool TryExecute<TState>(ThreadExecutionMode executionMode, Action<TState> handler, TState state, IReadOnlyMetadataContext? metadata)
        {
            if (CanExecuteInline(executionMode))
            {
                handler.Invoke(state);
                return true;
            }

            if (executionMode == ThreadExecutionMode.Main || executionMode == ThreadExecutionMode.MainAsync)
            {
                if (state == null)
                    _synchronizationContext.Post(o => ((Action<TState>) o).Invoke(default!), handler);
                else
                    _synchronizationContext.Post(handler.Invoke, state);
                return true;
            }

            if (executionMode == ThreadExecutionMode.Background)
            {
                if (state == null)
                    ThreadPool.QueueUserWorkItem(o => ((Action<TState>) o).Invoke(default!), handler);
                else
                    ThreadPool.QueueUserWorkItem(handler.Invoke, state);
                return true;
            }

            return false;
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