using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using MugenMvvm.Constants;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Commands;
using MugenMvvm.Interfaces.Commands.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Commands.Components
{
    public sealed class DelegateCommandExecutor<T> : ICommandExecutorComponent, ICommandConditionComponent, IHasDisposeCondition, IHasPriority
    {
        #region Fields

        private readonly bool _allowMultipleExecution;
        private readonly CommandExecutionBehavior _executionBehavior;
        private Delegate? _canExecute;
        private Delegate? _execute;
        private ICompositeCommand? _executingCommand;

        #endregion

        #region Constructors

        public DelegateCommandExecutor(Delegate execute, Delegate? canExecute, CommandExecutionBehavior executionBehavior, bool allowMultipleExecution)
        {
            Should.NotBeNull(execute, nameof(execute));
            Should.NotBeNull(executionBehavior, nameof(executionBehavior));
            _execute = execute;
            _canExecute = canExecute;
            _executionBehavior = executionBehavior;
            _allowMultipleExecution = allowMultipleExecution;
            CanDispose = true;
        }

        #endregion

        #region Properties

        public int Priority => CommandComponentPriority.Executor;

        public bool CanDispose { get; set; }

        #endregion

        #region Implementation of interfaces

        public bool HasCanExecute(ICompositeCommand command, IReadOnlyMetadataContext? metadata) => !_allowMultipleExecution || _canExecute != null;

        public bool CanExecute(ICompositeCommand command, object? parameter, IReadOnlyMetadataContext? metadata)
        {
            var canExecuteDelegate = _canExecute;
            if (canExecuteDelegate == null)
                return _execute != null;
            if (canExecuteDelegate is Func<bool> func)
                return func();
            return ((Func<T, bool>) canExecuteDelegate).Invoke((T) parameter!);
        }

        public async Task ExecuteAsync(ICompositeCommand command, object? parameter, IReadOnlyMetadataContext? metadata)
        {
            try
            {
                if (_allowMultipleExecution)
                {
                    await ExecuteInternalAsync(command, parameter).ConfigureAwait(false);
                    return;
                }

                if (Interlocked.CompareExchange(ref _executingCommand, command, null) != null)
                    return;

                command.RaiseCanExecuteChanged();
                await ExecuteInternalAsync(command, parameter).ConfigureAwait(false);
                _executingCommand = null;
                command.RaiseCanExecuteChanged();
            }
            catch
            {
                _executingCommand = null;
                throw;
            }
        }

        public void Dispose()
        {
            if (CanDispose)
            {
                _canExecute = null;
                _execute = null;
            }
        }

        #endregion

        #region Methods

        private async Task ExecuteInternalAsync(ICommand command, object? parameter)
        {
            var executeAction = _execute;
            if (executeAction == null)
                return;

            if (!_executionBehavior.BeforeExecute(command, parameter))
                return;

            if (executeAction is Action execute)
                execute();
            else if (executeAction is Action<T> genericExecute)
                genericExecute((T) parameter!);
            else if (executeAction is Func<Task> executeTask)
                await executeTask().ConfigureAwait(false);
            else
                await ((Func<T, Task>) executeAction).Invoke((T) parameter!).ConfigureAwait(false);

            _executionBehavior.AfterExecute(command, parameter);
        }

        #endregion
    }
}