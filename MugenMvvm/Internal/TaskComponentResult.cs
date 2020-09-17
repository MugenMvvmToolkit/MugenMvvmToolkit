using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Extensions;

namespace MugenMvvm.Internal
{
    internal struct TaskComponentResult<TComponent, TState, TResult>
        where TComponent : class
    {
        #region Fields

        private readonly CancellationToken _cancellationToken;
        private readonly TComponent[] _components;
        private readonly TResult _defaultResult;
        private readonly Func<TComponent, TState, CancellationToken, ValueTask<TResult>> _getResult;
        private readonly Func<TResult, TState, bool> _isValidResult;
        private readonly Action<TResult, TState>? _setResultCallback;
        private readonly TState _state;
        private object? _task;
        private TResult _result;
        private int _index;

        #endregion

        #region Constructors

        public TaskComponentResult(TComponent[] components, Func<TComponent, TState, CancellationToken, ValueTask<TResult>> getResult,
            Func<TResult, TState, bool> isValidResult, TState state, CancellationToken cancellationToken, TResult defaultResult, Action<TResult, TState>? setResultCallback)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(getResult, nameof(getResult));
            Should.NotBeNull(isValidResult, nameof(isValidResult));
            _components = components;
            _getResult = getResult;
            _isValidResult = isValidResult;
            _cancellationToken = cancellationToken;
            _state = state;
            _defaultResult = defaultResult;
            _setResultCallback = setResultCallback;
            _index = 0;
            _task = null;
            _result = default!;
            OnExecuted(null);
        }

        #endregion

        #region Methods

        public ValueTask<TResult> GetTask()
        {
            if (_task == null)
                return new ValueTask<TResult>(_result);
            if (_task is Task<TResult> t)
                return new ValueTask<TResult>(t);
            return new ValueTask<TResult>(((TaskImpl)_task).Task);
        }

        private void OnExecuted(Task<TResult>? task)
        {
            try
            {
                if (task != null && _isValidResult(task.Result, _state))
                {
                    SetResult(task.Result, null);
                    return;
                }

                if (_index >= _components.Length)
                {
                    SetResult(_defaultResult, null);
                    return;
                }

                _cancellationToken.ThrowIfCancellationRequested();

                var valueTask = _getResult(_components[_index++], _state, _cancellationToken);
                if (valueTask == default)
                {
                    OnExecuted(null);
                    return;
                }

                if (valueTask.IsCompleted && _isValidResult(valueTask.Result, _state))
                {
                    SetResult(valueTask.Result, null);
                    return;
                }

                if (_task == null)
                {
                    _task = new TaskImpl();
                    ((TaskImpl)_task).Result = this;
                }
                valueTask.AsTask().ContinueWith((t, state) => ((TaskImpl)state!).OnExecuted(t), _task, TaskContinuationOptions.ExecuteSynchronously);
            }
            catch (Exception e)
            {
                SetResult(default, e);
            }
        }

        private void SetResult([AllowNull] TResult result, Exception? exception)
        {
            if (_task is TaskImpl t)
            {
                if (exception == null)
                {
                    _setResultCallback?.Invoke(result!, _state);
                    t.TrySetResult(result!);
                }
                else
                    t.TrySetExceptionEx(exception);
            }
            else
            {
                if (exception == null)
                {
                    _setResultCallback?.Invoke(result!, _state);
                    _result = result!;
                }
                else
                    _task = exception.TryGetCanceledException(out var canceledException) ? Task.FromCanceled<TResult>(canceledException.CancellationToken) : Task.FromException<TResult>(exception);
            }
        }

        #endregion

        #region Nested types

        private sealed class TaskImpl : TaskCompletionSource<TResult>
        {
            #region Fields

            public TaskComponentResult<TComponent, TState, TResult> Result;

            #endregion

            #region Methods

            public void OnExecuted(Task<TResult> task) => Result.OnExecuted(task);

            #endregion
        }

        #endregion
    }
}