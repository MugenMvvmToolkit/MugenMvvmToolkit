using System;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Components;
using MugenMvvm.Constants;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Commands;
using MugenMvvm.Interfaces.Commands.Components;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Commands.Components
{
    public sealed class DelegateExecutorCommandComponent<T> : AttachableComponentBase<ICompositeCommand>, IExecutorCommandComponent, IConditionCommandComponent, IDisposable, IHasPriority
    {
        #region Fields

        private readonly bool _allowMultipleExecution;
        private readonly CommandExecutionMode _executionMode;
        private Delegate? _canExecute;
        private Delegate? _execute;
        private int _state;

        #endregion

        #region Constructors

        public DelegateExecutorCommandComponent(Delegate execute, Delegate? canExecute, CommandExecutionMode executionMode, bool allowMultipleExecution)
        {
            Should.NotBeNull(execute, nameof(execute));
            _execute = execute;
            _canExecute = canExecute;
            _executionMode = executionMode;
            _allowMultipleExecution = allowMultipleExecution;
        }

        #endregion

        #region Properties

        public int Priority => CommandComponentPriority.Executor;

        #endregion

        #region Implementation of interfaces

        public bool HasCanExecute()
        {
            return !_allowMultipleExecution || _canExecute != null;
        }

        public bool CanExecute(object? parameter)
        {
            var canExecuteDelegate = _canExecute;
            if (canExecuteDelegate == null)
                return _execute != null;
            if (canExecuteDelegate is Func<bool> func)
                return func();
            return ((Func<T, bool>)canExecuteDelegate).Invoke((T)parameter!);
        }

        public void Dispose()
        {
            _canExecute = null;
            _execute = null;
        }

        public Task ExecuteAsync(object? parameter)
        {
            if (_allowMultipleExecution)
                return ExecuteInternalAsync(parameter);

            if (Interlocked.CompareExchange(ref _state, int.MaxValue, 0) != 0)
                return Default.CompletedTask;

            try
            {
                var executionTask = ExecuteInternalAsync(parameter);
                if (executionTask.IsCompleted)
                {
                    _state = 0;
                    return executionTask;
                }

                if (IsAttached)
                    Owner.RaiseCanExecuteChanged();
                executionTask.ContinueWith((t, o) =>
                {
                    var component = (DelegateExecutorCommandComponent<T>)o;
                    component._state = 0;
                    component.Owner.RaiseCanExecuteChanged();
                }, this, TaskContinuationOptions.ExecuteSynchronously);
                return executionTask;
            }
            catch
            {
                _state = 0;
                throw;
            }
        }

        #endregion

        #region Methods

        private Task ExecuteInternalAsync(object? parameter)
        {
            if (IsAttached)
            {
                if (_executionMode == CommandExecutionMode.CanExecuteBeforeExecute)
                {
                    if (!Owner.CanExecute(parameter))
                    {
                        Owner.RaiseCanExecuteChanged();
                        return Default.CompletedTask;
                    }
                }
                else if (_executionMode == CommandExecutionMode.CanExecuteBeforeExecuteException)
                {
                    if (!Owner.CanExecute(parameter))
                        ExceptionManager.ThrowCommandCannotBeExecuted();
                }
            }

            var executeAction = _execute;
            if (executeAction == null)
                return Default.CompletedTask;

            if (executeAction is Action execute)
            {
                execute();
                return Default.CompletedTask;
            }

            if (executeAction is Action<T> genericExecute)
            {
                genericExecute((T)parameter!);
                return Default.CompletedTask;
            }

            if (executeAction is Func<Task> executeTask)
                return executeTask();
            return ((Func<T, Task>)executeAction).Invoke((T)parameter!);
        }

        #endregion
    }
}