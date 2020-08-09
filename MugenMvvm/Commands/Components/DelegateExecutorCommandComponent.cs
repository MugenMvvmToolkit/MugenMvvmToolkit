using System;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Constants;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Commands;
using MugenMvvm.Interfaces.Commands.Components;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;

namespace MugenMvvm.Commands.Components
{
    public sealed class DelegateExecutorCommandComponent<T> : IExecutorCommandComponent, IConditionCommandComponent, IDisposable, IHasPriority
    {
        #region Fields

        private readonly bool _allowMultipleExecution;
        private readonly CommandExecutionMode _executionMode;
        private Delegate? _canExecute;
        private Delegate? _execute;
        private ICompositeCommand? _executingCommand;

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

        public bool HasCanExecute(ICompositeCommand command) => !_allowMultipleExecution || _canExecute != null;

        public bool CanExecute(ICompositeCommand command, object? parameter)
        {
            var canExecuteDelegate = _canExecute;
            if (canExecuteDelegate == null)
                return _execute != null;
            if (canExecuteDelegate is Func<bool> func)
                return func();
            return ((Func<T, bool>) canExecuteDelegate).Invoke((T) parameter!);
        }

        public void Dispose()
        {
            _canExecute = null;
            _execute = null;
        }

        public Task ExecuteAsync(ICompositeCommand command, object? parameter)
        {
            if (_allowMultipleExecution)
                return ExecuteInternalAsync(command, parameter);

            if (Interlocked.CompareExchange(ref _executingCommand, command, null) != null)
                return Default.CompletedTask;

            try
            {
                var executionTask = ExecuteInternalAsync(command, parameter);
                if (executionTask.IsCompleted)
                {
                    _executingCommand = null;
                    return executionTask;
                }

                command.RaiseCanExecuteChanged();
                executionTask.ContinueWith((t, o) =>
                {
                    var component = (DelegateExecutorCommandComponent<T>) o!;
                    var cmd = component._executingCommand;
                    component._executingCommand = null;
                    cmd?.RaiseCanExecuteChanged();
                }, this, TaskContinuationOptions.ExecuteSynchronously);
                return executionTask;
            }
            catch
            {
                _executingCommand = null;
                throw;
            }
        }

        #endregion

        #region Methods

        private Task ExecuteInternalAsync(ICompositeCommand command, object? parameter)
        {
            if (_executionMode == CommandExecutionMode.CanExecuteBeforeExecute)
            {
                if (!command.CanExecute(parameter))
                {
                    command.RaiseCanExecuteChanged();
                    return Default.CompletedTask;
                }
            }
            else if (_executionMode == CommandExecutionMode.CanExecuteBeforeExecuteException)
            {
                if (!command.CanExecute(parameter))
                    ExceptionManager.ThrowCommandCannotBeExecuted();
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
                genericExecute((T) parameter!);
                return Default.CompletedTask;
            }

            if (executeAction is Func<Task> executeTask)
                return executeTask();
            return ((Func<T, Task>) executeAction).Invoke((T) parameter!);
        }

        #endregion
    }
}