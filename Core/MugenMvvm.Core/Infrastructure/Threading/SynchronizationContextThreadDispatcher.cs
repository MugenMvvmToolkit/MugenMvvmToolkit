using System;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Threading;

namespace MugenMvvm.Infrastructure.Threading
{
    public class SynchronizationContextThreadDispatcher : ThreadDispatcherBase
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

        #region Methods

        protected override Task? ExecuteOnMainThreadAsync(IThreadDispatcherHandler handler, object? state, bool includeTaskResult, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            if (includeTaskResult)
            {
                var closure = new Closure(handler);
                _synchronizationContext.Post(closure.Execute, state);
                return closure.Task;
            }

            if (state == null)
                _synchronizationContext.Post(o => ((IThreadDispatcherHandler) o).Execute(null), handler);
            else
                _synchronizationContext.Post(handler.ToExecuteDelegate, state);
            return Default.CompletedTask;
        }

        protected override Task? ExecuteOnMainThreadAsync(Action<object?> action, object? state, bool includeTaskResult, CancellationToken cancellationToken,
            IReadOnlyMetadataContext? metadata)
        {
            if (includeTaskResult)
            {
                var closure = new Closure(action);
                _synchronizationContext.Post(closure.Execute, state);
                return closure.Task;
            }

            if (state == null)
                _synchronizationContext.Post(o => ((Action<object?>) o).Invoke(null), action);
            else
                _synchronizationContext.Post(new SendOrPostCallback(action), state);
            return Default.CompletedTask;
        }

        protected override bool IsOnMainThread()
        {
            if (_mainThreadId == null)
                return SynchronizationContext.Current == _synchronizationContext;
            return _mainThreadId == Environment.CurrentManagedThreadId;
        }

        #endregion

        #region Nested types

        protected sealed class Closure : TaskCompletionSource<object?>
        {
            #region Fields

            private object? _handler;

            #endregion

            #region Constructors

            public Closure(Action<object> action)
            {
                _handler = action;
            }

            public Closure(IThreadDispatcherHandler handler)
            {
                _handler = handler;
            }

            #endregion

            #region Methods

            public void Execute(object? state)
            {
                try
                {
                    if (_handler is IThreadDispatcherHandler handler)
                        handler.Execute(state);
                    else
                        ((Action<object?>?) _handler)!.Invoke(state);
                    _handler = null;
                    TrySetResult(null);
                }
                catch (Exception e)
                {
                    TrySetException(e);
                }
            }

            #endregion
        }

        #endregion
    }
}