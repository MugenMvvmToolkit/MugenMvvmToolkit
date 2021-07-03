using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Components;
using MugenMvvm.Constants;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Commands;
using MugenMvvm.Interfaces.Commands.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Models.Components;
using MugenMvvm.Metadata;

namespace MugenMvvm.Commands.Components
{
    public sealed class DelegateCommandExecutor : MultiAttachableComponentBase<ICompositeCommand>, DelegateCommandExecutor.IDelegateCommandExecutor
    {
        private static readonly AllowMultipleExecutionDelegateCommandExecutor AllowMultipleExecutionExecutor = new();

        private volatile int _executeCount;
        private volatile ICompositeCommand? _lastExecutingCommand;

        private DelegateCommandExecutor()
        {
        }

        public bool IsExecuting => _executeCount != 0;

        public bool AllowMultipleExecution => false;

        public int Priority => CommandComponentPriority.Executor;

        public static ICommandExecutorComponent Get(bool allowMultipleExecution)
        {
            if (allowMultipleExecution)
                return AllowMultipleExecutionExecutor;
            return new DelegateCommandExecutor();
        }

        public static ICommandExecutorComponent Add<T>(ICompositeCommand command, Delegate execute, Delegate? canExecute, bool allowMultipleExecution)
        {
            Should.NotBeNull(command, nameof(command));
            var component = Get(allowMultipleExecution);
            command.AddComponent(component);
            command.AddComponent(canExecute == null ? new DelegateExecutor<T>(execute) : new ConditionDelegateExecutor<T>(execute, canExecute));
            return component;
        }

        public static void SynchronizeExecution(ICompositeCommand command, ICompositeCommand value)
        {
            Should.NotBeNull(command, nameof(command));
            Should.NotBeNull(value, nameof(value));
            var executor1 = command.GetComponentOptional<IDelegateCommandExecutor>();
            var executor2 = value.GetComponentOptional<IDelegateCommandExecutor>();
            if (ReferenceEquals(executor1, executor2) && executor1 != null && !executor1.AllowMultipleExecution)
                return;

            if (executor1 != null && !executor1.AllowMultipleExecution)
            {
                value.RemoveComponents<IDelegateCommandExecutor>();
                value.AddComponent(executor1);
                return;
            }

            if (executor2 != null && !executor2.AllowMultipleExecution)
            {
                command.RemoveComponents<IDelegateCommandExecutor>();
                command.AddComponent(executor2);
                return;
            }

            var executor = new DelegateCommandExecutor();
            command.RemoveComponents<IDelegateCommandExecutor>();
            value.RemoveComponents<IDelegateCommandExecutor>();
            command.AddComponent(executor);
            value.AddComponent(executor);
        }

        public bool CanExecute(ICompositeCommand command, object? parameter, IReadOnlyMetadataContext? metadata) =>
            (_executeCount == 0 || IsForceExecute(command, metadata)) && CanExecuteInternal(command, parameter, metadata);

        public async ValueTask<bool> TryExecuteAsync(ICompositeCommand command, object? parameter, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            try
            {
                if (Interlocked.Increment(ref _executeCount) == 1)
                    RaiseCanExecuteChanged(metadata);
                else if (!IsForceExecute(command, metadata))
                    return false;

                Interlocked.Exchange(ref _lastExecutingCommand, command);
                return await ExecuteInternalAsync(command, parameter, true, cancellationToken, metadata).ConfigureAwait(false);
            }
            finally
            {
                if (Interlocked.Decrement(ref _executeCount) == 0)
                {
                    if (_executeCount == 0)
                        Interlocked.Exchange(ref _lastExecutingCommand, null); //possible race condition, not critical for this case.
                    RaiseCanExecuteChanged(metadata);
                }
            }
        }

        private static ValueTask<bool> ExecuteInternalAsync(ICompositeCommand command, object? parameter, bool force, CancellationToken cancellationToken,
            IReadOnlyMetadataContext? metadata)
        {
            if (cancellationToken.IsCancellationRequested)
                return default;

            var executor = command.GetComponentOptional<IDelegateExecutor>();
            if (executor == null || !command.CanExecute(parameter, force ? MugenExtensions.GetForceExecuteMetadata(metadata) : metadata))
            {
                if (!force)
                    command.RaiseCanExecuteChanged(metadata);
                return default;
            }

            return executor.ExecuteAsync(command, parameter, cancellationToken, metadata);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool CanExecuteInternal(ICompositeCommand command, object? parameter, IReadOnlyMetadataContext? metadata)
        {
            var executor = command.GetComponentOptional<IDelegateExecutor>();
            return executor != null && executor.CanExecute(command, parameter, metadata);
        }

        private bool IsForceExecute(ICompositeCommand command, IReadOnlyMetadataContext? metadata)
        {
            if (command.HasMetadata && command.Metadata.TryGet(CommandMetadata.CanForceExecute, out var func))
            {
                var b = func(_lastExecutingCommand, metadata);
                if (b.HasValue)
                    return b.Value;
            }

            return metadata != null && metadata.TryGet(CommandMetadata.ForceExecute, out var value) && value;
        }

        private void RaiseCanExecuteChanged(IReadOnlyMetadataContext? metadata)
        {
            foreach (var owner in Owners)
                owner.RaiseCanExecuteChanged(metadata);
        }

        public interface IDelegateExecutor : IComponent<ICompositeCommand>
        {
            bool CanExecute(ICompositeCommand command, object? parameter, IReadOnlyMetadataContext? metadata);

            public ValueTask<bool> ExecuteAsync(ICompositeCommand command, object? parameter, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata);
        }

        internal interface IDelegateCommandExecutor : ICommandExecutorComponent, ICommandConditionComponent, IHasPriority
        {
            bool AllowMultipleExecution { get; }
        }

        public class DelegateExecutor<T> : IDelegateExecutor, IDisposableComponent<ICompositeCommand>
        {
            private Delegate? _execute;

            public DelegateExecutor(Delegate execute)
            {
                Should.NotBeNull(execute, nameof(execute));
                _execute = execute;
            }

            public virtual bool CanExecute(ICompositeCommand command, object? parameter, IReadOnlyMetadataContext? metadata) => _execute != null;

            public virtual async ValueTask<bool> ExecuteAsync(ICompositeCommand command, object? parameter, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
            {
                var executeAction = _execute;
                if (executeAction == null || cancellationToken.IsCancellationRequested)
                    return false;

                if (executeAction is Action<IReadOnlyMetadataContext?> execute)
                {
                    execute(metadata);
                    return true;
                }

                if (executeAction is Action<T, IReadOnlyMetadataContext?> genericExecute)
                {
                    genericExecute((T)parameter!, metadata);
                    return true;
                }

                if (executeAction is Func<CancellationToken, IReadOnlyMetadataContext?, Task> executeTask)
                {
                    await executeTask(cancellationToken, metadata).ConfigureAwait(false);
                    return true;
                }

                if (executeAction is Func<T, CancellationToken, IReadOnlyMetadataContext?, Task> executeTaskParameter)
                {
                    await executeTaskParameter((T)parameter!, cancellationToken, metadata).ConfigureAwait(false);
                    return true;
                }

                if (executeAction is Func<CancellationToken, IReadOnlyMetadataContext?, ValueTask<bool>> executeTaskBool)
                    return await executeTaskBool(cancellationToken, metadata).ConfigureAwait(false);

                return await ((Func<T, CancellationToken, IReadOnlyMetadataContext?, ValueTask<bool>>)executeAction)
                             .Invoke((T)parameter!, cancellationToken, metadata)
                             .ConfigureAwait(false);
            }

            public virtual void Dispose(ICompositeCommand owner, IReadOnlyMetadataContext? metadata) => _execute = null;
        }

        public class ConditionDelegateExecutor<T> : DelegateExecutor<T>
        {
            private Delegate? _canExecute;

            public ConditionDelegateExecutor(Delegate execute, Delegate canExecute) : base(execute)
            {
                Should.NotBeNull(canExecute, nameof(canExecute));
                _canExecute = canExecute;
            }

            public override bool CanExecute(ICompositeCommand command, object? parameter, IReadOnlyMetadataContext? metadata)
            {
                var canExecuteDelegate = _canExecute;
                if (canExecuteDelegate == null)
                    return false;

                if (canExecuteDelegate is Func<IReadOnlyMetadataContext?, bool> func)
                    return func(metadata);
                return ((Func<T, IReadOnlyMetadataContext?, bool>)canExecuteDelegate).Invoke((T)parameter!, metadata);
            }

            public override void Dispose(ICompositeCommand owner, IReadOnlyMetadataContext? metadata)
            {
                base.Dispose(owner, metadata);
                _canExecute = null;
            }
        }

        private sealed class AllowMultipleExecutionDelegateCommandExecutor : IDelegateCommandExecutor
        {
            public bool AllowMultipleExecution => true;

            public int Priority => CommandComponentPriority.Executor;

            public bool CanExecute(ICompositeCommand command, object? parameter, IReadOnlyMetadataContext? metadata) => CanExecuteInternal(command, parameter, metadata);

            public ValueTask<bool> TryExecuteAsync(ICompositeCommand command, object? parameter, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata) =>
                ExecuteInternalAsync(command, parameter, false, cancellationToken, metadata);
        }
    }
}