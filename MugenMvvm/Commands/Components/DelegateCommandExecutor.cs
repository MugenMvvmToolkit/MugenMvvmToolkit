using System;
using System.Threading;
using System.Threading.Tasks;
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
        private readonly bool _allowMultipleExecution;
        private readonly CommandExecutionBehavior _executionBehavior;
        private Delegate? _canExecute;
        private Delegate? _execute;
        private ICompositeCommand? _executingCommand;
        private bool _executing;

        public DelegateCommandExecutor(Delegate execute, Delegate? canExecute, CommandExecutionBehavior executionBehavior, bool allowMultipleExecution)
        {
            Should.NotBeNull(execute, nameof(execute));
            Should.NotBeNull(executionBehavior, nameof(executionBehavior));
            _execute = execute;
            _canExecute = canExecute;
            _executionBehavior = executionBehavior;
            _allowMultipleExecution = allowMultipleExecution;
            IsDisposable = true;
        }

        public bool IsDisposable { get; set; }

        public int Priority => CommandComponentPriority.Executor;

        public bool CanExecute(ICompositeCommand command, object? parameter, IReadOnlyMetadataContext? metadata)
        {
            var canExecuteDelegate = _canExecute;
            if (canExecuteDelegate == null)
                return !_executing && _execute != null;
            if (canExecuteDelegate is Func<IReadOnlyMetadataContext?, bool> func)
                return func(metadata);
            return ((Func<T, IReadOnlyMetadataContext?, bool>) canExecuteDelegate).Invoke((T) parameter!, metadata);
        }

        public async ValueTask<bool> ExecuteAsync(ICompositeCommand command, object? parameter, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            try
            {
                if (_allowMultipleExecution)
                    return await ExecuteInternalAsync(command, parameter, cancellationToken, metadata).ConfigureAwait(false);

                if (Interlocked.CompareExchange(ref _executingCommand, command, null) != null)
                    return false;

                cancellationToken.ThrowIfCancellationRequested();
                command.RaiseCanExecuteChanged();
                var result = await ExecuteInternalAsync(command, parameter, cancellationToken, metadata).ConfigureAwait(false);
                _executingCommand = null;
                _executing = false;
                command.RaiseCanExecuteChanged();
                return result;
            }
            catch
            {
                _executing = false;
                _executingCommand = null;
                command.RaiseCanExecuteChanged();
                throw;
            }
        }

        public void Dispose()
        {
            if (IsDisposable)
            {
                _canExecute = null;
                _execute = null;
            }
        }

        private async ValueTask<bool> ExecuteInternalAsync(ICompositeCommand command, object? parameter, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            var executeAction = _execute;
            if (executeAction == null || cancellationToken.IsCancellationRequested)
                return false;

            if (!_executionBehavior.BeforeExecute(command, parameter, cancellationToken, metadata))
                return false;
            if (!_allowMultipleExecution)
                _executing = true;

            if (executeAction is Action<IReadOnlyMetadataContext?> execute)
                execute(metadata);
            else if (executeAction is Action<T, IReadOnlyMetadataContext?> genericExecute)
                genericExecute((T) parameter!, metadata);
            else if (executeAction is Func<CancellationToken, IReadOnlyMetadataContext?, Task> executeTask)
                await executeTask(cancellationToken, metadata).ConfigureAwait(false);
            else
                await ((Func<T, CancellationToken, IReadOnlyMetadataContext?, Task>) executeAction).Invoke((T) parameter!, cancellationToken, metadata).ConfigureAwait(false);

            _executionBehavior.AfterExecute(command, parameter, cancellationToken, metadata);
            return true;
        }
    }
}