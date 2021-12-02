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
using MugenMvvm.Internal;
using MugenMvvm.Metadata;

#pragma warning disable 4014

namespace MugenMvvm.Commands.Components
{
    public sealed class DelegateCommandExecutor<T> : ICommandExecutorComponent, ICommandConditionComponent, IDisposableComponent<ICompositeCommand>, IHasPriority
    {
        private readonly bool _allowMultipleExecution;
        private Delegate? _execute;
        private Delegate? _canExecute;
        private volatile int _executeCount;
        private volatile int _currentThreadId;
        private volatile TaskCompletionSource<object?>? _tcs;
        private CancellationTokenSource? _cancellationTokenSource;

        public DelegateCommandExecutor(Delegate execute, Delegate? canExecute, bool allowMultipleExecution)
        {
            Should.NotBeNull(execute, nameof(execute));
            _execute = execute;
            _canExecute = canExecute;
            _allowMultipleExecution = allowMultipleExecution;
            _currentThreadId = int.MinValue;
            IsRecursiveExecutionSupported = true;
            IsAsync = _execute is Func<CancellationToken, IReadOnlyMetadataContext?, Task<bool>>
                or Func<T, CancellationToken, IReadOnlyMetadataContext?, Task<bool>>
                or Func<CancellationToken, IReadOnlyMetadataContext?, Task>
                or Func<T, CancellationToken, IReadOnlyMetadataContext?, Task>;
        }

        public int Priority => CommandComponentPriority.Executor;

        public bool IsRecursiveExecutionSupported { get; set; }

        private bool IsAsync { get; }

        public bool CanExecute(ICompositeCommand command, object? parameter, IReadOnlyMetadataContext? metadata) =>
            (_allowMultipleExecution || _executeCount == 0 || IsRecursiveExecution(metadata, out _) || IsForceExecute(metadata)) && CanExecute(parameter, metadata);

        public bool IsExecuting(ICompositeCommand command, IReadOnlyMetadataContext? metadata) => _executeCount != 0;

        public async Task<bool> TryExecuteAsync(ICompositeCommand command, object? parameter, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            CancellationTokenSource? cts = null;
            try
            {
                if (Interlocked.Increment(ref _executeCount) == 1)
                    command.RaiseCanExecuteChanged(metadata);
                else if (!_allowMultipleExecution && !IsRecursiveExecution(metadata, out cts) && !IsForceExecute(metadata))
                    return false;

                if (!_allowMultipleExecution)
                {
                    if (IsAsync)
                    {
                        var isNew = cts == null;
                        cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, isNew ? default : cts!.Token);
                        if (isNew)
                        {
                            Interlocked.Exchange(ref _cancellationTokenSource, cts).SafeCancel();
                            if (IsRecursiveExecutionSupported)
                                metadata = metadata.WithValue(CommandMetadata.ExecutorToken, cts);
                        }

                        cancellationToken = cts.Token;
                    }
                    else if (IsRecursiveExecutionSupported)
                        Interlocked.Exchange(ref _currentThreadId, Environment.CurrentManagedThreadId);
                }

                return await ExecuteAsync(parameter, cancellationToken, metadata).ConfigureAwait(false);
            }
            finally
            {
                if (cts != null)
                {
                    Interlocked.CompareExchange(ref _cancellationTokenSource, null, cts);
                    cts.Dispose();
                }

                if (Interlocked.Decrement(ref _executeCount) == 0)
                {
                    Interlocked.Exchange(ref _tcs, null)?.TrySetResult(null);
                    if (!_allowMultipleExecution && !IsAsync && IsRecursiveExecutionSupported)
                        Interlocked.CompareExchange(ref _currentThreadId, int.MinValue, Environment.CurrentManagedThreadId);
                    command.RaiseCanExecuteChanged(metadata);
                }
            }
        }

        public Task TryWaitAsync(ICompositeCommand command, IReadOnlyMetadataContext? metadata)
        {
            if (_executeCount == 0)
                return Task.CompletedTask;

            var tcs = _tcs;
            if (tcs == null)
            {
                tcs = new TaskCompletionSource<object?>();
                tcs = Interlocked.CompareExchange(ref _tcs, tcs, null) ?? tcs;
            }

            if (_executeCount == 0)
            {
                tcs.TrySetResult(null);
                Interlocked.CompareExchange(ref _tcs, null, tcs);
            }

            return tcs.Task;
        }

        public void OnDisposing(ICompositeCommand owner, IReadOnlyMetadataContext? metadata)
        {
        }

        public void OnDisposed(ICompositeCommand owner, IReadOnlyMetadataContext? metadata)
        {
            _cancellationTokenSource.SafeCancel();
            _execute = null;
            _canExecute = null;
        }

        private static bool IsForceExecute(IReadOnlyMetadataContext? metadata) => metadata != null &&
                                                                                  (ReferenceEquals(metadata, MugenExtensions.ForceExecuteMetadata) ||
                                                                                   metadata.TryGet(CommandMetadata.ForceExecute, out var value) && value);

        private bool IsRecursiveExecution(IReadOnlyMetadataContext? metadata, out CancellationTokenSource? cts)
        {
            cts = null;
            if (!IsRecursiveExecutionSupported)
                return false;
            if (_allowMultipleExecution)
                return false;

            if (IsAsync)
                return metadata != null && metadata.TryGet(CommandMetadata.ExecutorToken, out cts) && ReferenceEquals(Volatile.Read(ref _cancellationTokenSource), cts);
            return _currentThreadId == Environment.CurrentManagedThreadId;
        }

        private bool CanExecute(object? parameter, IReadOnlyMetadataContext? metadata)
        {
            var canExecuteDelegate = _canExecute;
            if (canExecuteDelegate == null)
                return _execute != null;

            if (canExecuteDelegate is Func<IReadOnlyMetadataContext?, bool> func)
                return func(metadata);
            return ((Func<T, IReadOnlyMetadataContext?, bool>) canExecuteDelegate).Invoke((T) parameter!, metadata);
        }

        private async Task<bool> ExecuteAsync(object? parameter, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            if (cancellationToken.IsCancellationRequested || !TypeChecker.IsCompatible<T>(parameter))
                return false;

            var executeAction = _execute;
            if (executeAction == null || cancellationToken.IsCancellationRequested)
                return false;

            if (IsAsync)
            {
                if (executeAction is Func<CancellationToken, IReadOnlyMetadataContext?, Task<bool>> executeTaskBool)
                    return await executeTaskBool(cancellationToken, metadata).ConfigureAwait(false);

                if (executeAction is Func<T, CancellationToken, IReadOnlyMetadataContext?, Task<bool>> executeTaskBoolArg)
                    return await executeTaskBoolArg((T) parameter!, cancellationToken, metadata).ConfigureAwait(false);

                if (executeAction is Func<CancellationToken, IReadOnlyMetadataContext?, Task> executeTask)
                    await executeTask(cancellationToken, metadata).ConfigureAwait(false);
                else
                    await ((Func<T, CancellationToken, IReadOnlyMetadataContext?, Task>) executeAction).Invoke((T) parameter!, cancellationToken, metadata).ConfigureAwait(false);
                return true;
            }

            if (executeAction is Action<IReadOnlyMetadataContext?> execute)
                execute(metadata);
            else
                ((Action<T, IReadOnlyMetadataContext?>) executeAction).Invoke((T) parameter!, metadata);
            return true;
        }
    }
}