using System;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Collections;
using MugenMvvm.Components;
using MugenMvvm.Constants;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Commands;
using MugenMvvm.Interfaces.Commands.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Models.Components;
using MugenMvvm.Internal;
using MugenMvvm.Metadata;

namespace MugenMvvm.Commands.Components
{
    public sealed class DelegateCommandExecutor : MultiAttachableComponentBase<ICompositeCommand>, DelegateCommandExecutor.IDelegateCommandExecutor
    {
        private static readonly AllowMultipleExecutionDelegateCommandExecutor AllowMultipleExecutionExecutor = new();

        private volatile int _executeCount;
        private volatile ICompositeCommand? _executingCommand;

        private DelegateCommandExecutor()
        {
        }

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

        public static void Synchronize(ICompositeCommand command, ICompositeCommand target, bool bidirectional)
        {
            Should.NotBeNull(command, nameof(command));
            Should.NotBeNull(target, nameof(target));
            Synchronize(command, target);
            if (!bidirectional)
                AddRemoveForceExecuteCommands(target.Metadata, command, false);
        }

        public bool CanExecute(ICompositeCommand command, object? parameter, IReadOnlyMetadataContext? metadata) =>
            (_executeCount == 0 || IsForceExecute(command, metadata)) && CanExecuteInternal(command, parameter, metadata);

        public bool IsExecuting(ICompositeCommand command, IReadOnlyMetadataContext? metadata) => _executeCount != 0;

        public async Task<bool> TryExecuteAsync(ICompositeCommand command, object? parameter, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            try
            {
                if (Interlocked.Increment(ref _executeCount) == 1)
                    RaiseCanExecuteChanged(metadata);
                else if (!IsForceExecute(command, metadata))
                    return false;

                Interlocked.Exchange(ref _executingCommand, command);
                return await ExecuteInternalAsync(command, parameter, true, cancellationToken, metadata).ConfigureAwait(false);
            }
            finally
            {
                if (Interlocked.Decrement(ref _executeCount) == 0)
                {
                    if (_executeCount == 0)
                        Interlocked.Exchange(ref _executingCommand, null); //possible race condition, not critical for this case.
                    RaiseCanExecuteChanged(metadata);
                }
            }
        }

        protected override void OnDetached(ICompositeCommand owner, IReadOnlyMetadataContext? metadata)
        {
            foreach (var command in Owners)
            {
                if (command.HasMetadata)
                    AddRemoveForceExecuteCommands(command.Metadata, owner, true);
            }

            base.OnDetached(owner, metadata);
        }

        private static void Synchronize(ICompositeCommand command, ICompositeCommand target)
        {
            var executor1 = command.GetComponentOptional<IDelegateCommandExecutor>();
            var executor2 = target.GetComponentOptional<IDelegateCommandExecutor>();
            if (ReferenceEquals(executor1, executor2) && executor1 != null && !executor1.AllowMultipleExecution)
                return;

            if (executor1 != null && !executor1.AllowMultipleExecution)
            {
                target.RemoveComponents<IDelegateCommandExecutor>();
                target.AddComponent(executor1);
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
            target.RemoveComponents<IDelegateCommandExecutor>();
            command.AddComponent(executor);
            target.AddComponent(executor);
        }

        private static void AddRemoveForceExecuteCommands(IMetadataContext metadata, ICompositeCommand command, bool remove)
        {
            if (remove && !metadata.Contains(InternalMetadata.AllowForceExecuteCommands))
                return;

            metadata.AddOrUpdate(InternalMetadata.AllowForceExecuteCommands, command, (command, remove), (_, _, currentValue, s) =>
            {
                var editor = ItemOrListEditor<object>.FromRawValue(currentValue);
                if (s.remove)
                    editor.Remove(s.command);
                else
                {
                    editor.DefaultCapacity = 2;
                    editor.Add(s.command);
                }

                return editor.GetRawValueInternal();
            });
        }

        private static Task<bool> ExecuteInternalAsync(ICompositeCommand command, object? parameter, bool force, CancellationToken cancellationToken,
            IReadOnlyMetadataContext? metadata)
        {
            if (cancellationToken.IsCancellationRequested)
                return Default.FalseTask;

            var executor = command.GetComponentOptional<IDelegateExecutor>();
            if (executor == null || !command.CanExecute(parameter, force ? MugenExtensions.GetForceExecuteMetadata(metadata) : metadata))
            {
                if (!force)
                    command.RaiseCanExecuteChanged(metadata);
                return Default.FalseTask;
            }

            return executor.ExecuteAsync(command, parameter, cancellationToken, metadata);
        }

        private static bool CanExecuteInternal(ICompositeCommand command, object? parameter, IReadOnlyMetadataContext? metadata)
        {
            var executor = command.GetComponentOptional<IDelegateExecutor>();
            return executor != null && executor.CanExecute(command, parameter, metadata);
        }

        private bool IsForceExecute(ICompositeCommand command, IReadOnlyMetadataContext? metadata)
        {
            if (command.HasMetadata && command.Metadata.TryGet(InternalMetadata.AllowForceExecuteCommands, out var commands))
            {
                var executingCommand = _executingCommand;
                if (executingCommand != null && ItemOrIReadOnlyList.FromRawValue<ICompositeCommand>(commands).Contains(executingCommand))
                    return true;
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

            public Task<bool> ExecuteAsync(ICompositeCommand command, object? parameter, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata);
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

            public virtual async Task<bool> ExecuteAsync(ICompositeCommand command, object? parameter, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
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
                    genericExecute((T) parameter!, metadata);
                    return true;
                }

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
                return ((Func<T, IReadOnlyMetadataContext?, bool>) canExecuteDelegate).Invoke((T) parameter!, metadata);
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

            public bool IsExecuting(ICompositeCommand command, IReadOnlyMetadataContext? metadata) => false;

            public Task<bool> TryExecuteAsync(ICompositeCommand command, object? parameter, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata) =>
                ExecuteInternalAsync(command, parameter, false, cancellationToken, metadata);
        }
    }
}