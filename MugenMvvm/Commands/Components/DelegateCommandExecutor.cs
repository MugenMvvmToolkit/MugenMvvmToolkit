using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Constants;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Commands;
using MugenMvvm.Interfaces.Commands.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Models.Components;
using MugenMvvm.Metadata;

namespace MugenMvvm.Commands.Components
{
    public sealed class DelegateCommandExecutor<T> : ICommandExecutorComponent, ICommandConditionComponent, IDisposableComponent<ICompositeCommand>, IHasDisposeCondition,
        IHasPriority
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsForceExecute(IReadOnlyMetadataContext? metadata) => metadata != null && metadata.TryGet(CommandMetadata.ForceExecute, out var value) && value;

        public bool CanExecute(ICompositeCommand command, object? parameter, IReadOnlyMetadataContext? metadata)
        {
            if (_execute == null || _executing && !IsForceExecute(metadata))
                return false;

            var canExecuteDelegate = _canExecute;
            if (canExecuteDelegate == null)
                return true;

            if (canExecuteDelegate is Func<IReadOnlyMetadataContext?, bool> func)
                return func(metadata);
            return ((Func<T, IReadOnlyMetadataContext?, bool>)canExecuteDelegate).Invoke((T)parameter!, metadata);
        }

        public async ValueTask<bool> ExecuteAsync(ICompositeCommand command, object? parameter, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            try
            {
                if (_allowMultipleExecution)
                    return await ExecuteInternalAsync(command, parameter, cancellationToken, metadata).ConfigureAwait(false);

                if (Interlocked.CompareExchange(ref _executingCommand, command, null) != null)
                {
                    if (!IsForceExecute(metadata))
                        return false;
                    _executingCommand = command;
                }

                cancellationToken.ThrowIfCancellationRequested();
                command.RaiseCanExecuteChanged();
                var result = await ExecuteInternalAsync(command, parameter, cancellationToken, metadata).ConfigureAwait(false);
                OnExecuted(command);
                return result;
            }
            catch
            {
                OnExecuted(command);
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

        private void OnExecuted(ICompositeCommand command)
        {
            if (Interlocked.CompareExchange(ref _executingCommand, null, command) == command)
                _executing = false;
            command.RaiseCanExecuteChanged();
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

            var result = true;
            if (executeAction is Action<IReadOnlyMetadataContext?> execute)
                execute(metadata);
            else if (executeAction is Action<T, IReadOnlyMetadataContext?> genericExecute)
                genericExecute((T)parameter!, metadata);
            else if (executeAction is Func<CancellationToken, IReadOnlyMetadataContext?, Task> executeTask)
                await executeTask(cancellationToken, metadata).ConfigureAwait(false);
            else if (executeAction is Func<T, CancellationToken, IReadOnlyMetadataContext?, Task> executeTaskParameter)
                await executeTaskParameter((T)parameter!, cancellationToken, metadata).ConfigureAwait(false);
            else if (executeAction is Func<CancellationToken, IReadOnlyMetadataContext?, ValueTask<bool>> executeTaskBool)
                result = await executeTaskBool(cancellationToken, metadata).ConfigureAwait(false);
            else
            {
                result = await ((Func<T, CancellationToken, IReadOnlyMetadataContext?, ValueTask<bool>>)executeAction)
                               .Invoke((T)parameter!, cancellationToken, metadata)
                               .ConfigureAwait(false);
            }

            _executionBehavior.AfterExecute(command, parameter, cancellationToken, metadata);
            return result;
        }

        void IDisposableComponent<ICompositeCommand>.Dispose(ICompositeCommand owner, IReadOnlyMetadataContext? metadata) => Dispose();
    }
}