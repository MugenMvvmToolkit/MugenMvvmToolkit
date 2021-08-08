using System;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Constants;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Commands;
using MugenMvvm.Interfaces.Commands.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Models.Components;
using MugenMvvm.Metadata;

namespace MugenMvvm.Commands.Components
{
    public sealed class DelegateCommandExecutor<T> : ICommandExecutorComponent, ICommandConditionComponent, IDisposableComponent<ICompositeCommand>, IHasPriority
    {
        private readonly bool _allowMultipleExecution;
        private Delegate? _execute;
        private Delegate? _canExecute;
        private volatile int _executeCount;
        private CancellationTokenSource? _cancellationTokenSource;

        public DelegateCommandExecutor(Delegate execute, Delegate? canExecute, bool allowMultipleExecution)
        {
            Should.NotBeNull(execute, nameof(execute));
            _execute = execute;
            _canExecute = canExecute;
            _allowMultipleExecution = allowMultipleExecution;
            IsAsync = _execute is Func<CancellationToken, IReadOnlyMetadataContext?, Task<bool>>
                or Func<T, CancellationToken, IReadOnlyMetadataContext?, Task<bool>>
                or Func<CancellationToken, IReadOnlyMetadataContext?, Task>
                or Func<T, CancellationToken, IReadOnlyMetadataContext?, Task>;
        }

        public int Priority => CommandComponentPriority.Executor;

        private bool IsAsync { get; }

        public bool CanExecute(ICompositeCommand command, object? parameter, IReadOnlyMetadataContext? metadata) =>
            (_allowMultipleExecution || _executeCount == 0 || IsForceExecute(metadata)) && CanExecute(parameter, metadata);

        public bool IsExecuting(ICompositeCommand command, IReadOnlyMetadataContext? metadata) => _executeCount != 0;

        public async Task<bool> TryExecuteAsync(ICompositeCommand command, object? parameter, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            CancellationTokenSource? cts = null;
            try
            {
                if (Interlocked.Increment(ref _executeCount) == 1)
                    command.RaiseCanExecuteChanged(metadata);
                else if (!_allowMultipleExecution && !IsForceExecute(metadata))
                    return false;

                cts = !_allowMultipleExecution && IsAsync ? CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, default) : null;
                Interlocked.Exchange(ref _cancellationTokenSource, cts)?.Cancel();
                return await ExecuteAsync(parameter, cts?.Token ?? cancellationToken, metadata).ConfigureAwait(false);
            }
            finally
            {
                if (cts != null)
                {
                    Interlocked.CompareExchange(ref _cancellationTokenSource, null, cts);
                    cts.Dispose();
                }

                if (Interlocked.Decrement(ref _executeCount) == 0)
                    command.RaiseCanExecuteChanged(metadata);
            }
        }

        public void Dispose(ICompositeCommand owner, IReadOnlyMetadataContext? metadata)
        {
            _execute = null;
            _canExecute = null;
        }

        private static bool IsForceExecute(IReadOnlyMetadataContext? metadata) => metadata != null &&
                                                                                  (ReferenceEquals(metadata, MugenExtensions.ForceExecuteMetadata) ||
                                                                                   metadata.TryGet(CommandMetadata.ForceExecute, out var value) && value);

        private bool CanExecute(object? parameter, IReadOnlyMetadataContext? metadata)
        {
            var canExecuteDelegate = _canExecute;
            if (canExecuteDelegate == null)
                return _execute != null;

            if (canExecuteDelegate is Func<IReadOnlyMetadataContext?, bool> func)
                return func(metadata);
            return ((Func<T, IReadOnlyMetadataContext?, bool>)canExecuteDelegate).Invoke((T)parameter!, metadata);
        }

        private async Task<bool> ExecuteAsync(object? parameter, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            if (cancellationToken.IsCancellationRequested)
                return false;

            var executeAction = _execute;
            if (executeAction == null || cancellationToken.IsCancellationRequested)
                return false;

            if (IsAsync)
            {
                if (executeAction is Func<CancellationToken, IReadOnlyMetadataContext?, Task<bool>> executeTaskBool)
                    return await executeTaskBool(cancellationToken, metadata).ConfigureAwait(false);

                if (executeAction is Func<T, CancellationToken, IReadOnlyMetadataContext?, Task<bool>> executeTaskBoolArg)
                    return await executeTaskBoolArg((T)parameter!, cancellationToken, metadata).ConfigureAwait(false);

                if (executeAction is Func<CancellationToken, IReadOnlyMetadataContext?, Task> executeTask)
                    await executeTask(cancellationToken, metadata).ConfigureAwait(false);
                else
                    await ((Func<T, CancellationToken, IReadOnlyMetadataContext?, Task>)executeAction).Invoke((T)parameter!, cancellationToken, metadata).ConfigureAwait(false);
                return true;
            }

            if (executeAction is Action<IReadOnlyMetadataContext?> execute)
                execute(metadata);
            else
                ((Action<T, IReadOnlyMetadataContext?>)executeAction).Invoke((T)parameter!, metadata);
            return true;
        }
    }
}