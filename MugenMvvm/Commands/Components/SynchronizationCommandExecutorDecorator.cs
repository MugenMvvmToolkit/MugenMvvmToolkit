using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Collections;
using MugenMvvm.Components;
using MugenMvvm.Constants;
using MugenMvvm.Extensions;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Commands;
using MugenMvvm.Interfaces.Commands.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Metadata;

namespace MugenMvvm.Commands.Components
{
    public sealed class SynchronizationCommandExecutorDecorator : MultiAttachableComponentBase<ICompositeCommand>, ICommandConditionComponent, IHasPriority
    {
        private volatile ICompositeCommand? _executingCommand;
        private CancellationTokenSource? _cancellationTokenSource;

        private SynchronizationCommandExecutorDecorator(int priority)
        {
            Priority = priority;
        }

        public int Priority { get; }

        public static void Synchronize(ICompositeCommand command, ICompositeCommand target, bool bidirectional, int priority = CommandComponentPriority.SynchronizationDecorator)
        {
            if (ReferenceEquals(command, target))
                return;
            Should.NotBeNull(command, nameof(command));
            Should.NotBeNull(target, nameof(target));
            Synchronize(command, target, priority);
            if (!bidirectional)
                AddRemoveForceExecuteCommands(target.Metadata, command, false);
        }

        public bool CanExecute(ICompositeCommand command, object? parameter, IReadOnlyMetadataContext? metadata)
        {
            var executingCommand = _executingCommand;
            return executingCommand == null || IsForceExecute(command, executingCommand, metadata);
        }

        protected override void OnAttached(ICompositeCommand owner, IReadOnlyMetadataContext? metadata)
        {
            base.OnAttached(owner, metadata);
            owner.AddComponent(new CommandExecutorInterceptor(this, Priority));
        }

        protected override void OnDetached(ICompositeCommand owner, IReadOnlyMetadataContext? metadata)
        {
            base.OnDetached(owner, metadata);
            owner.RemoveComponents<CommandExecutorInterceptor>();
            foreach (var command in Owners)
            {
                if (command.HasMetadata)
                    AddRemoveForceExecuteCommands(command.Metadata, owner, true);
            }
        }

        private static void Synchronize(ICompositeCommand command, ICompositeCommand target, int priority)
        {
            var synchronizer1 = command.GetComponentOptional<SynchronizationCommandExecutorDecorator>();
            var synchronizer2 = target.GetComponentOptional<SynchronizationCommandExecutorDecorator>();
            if (ReferenceEquals(synchronizer1, synchronizer2) && synchronizer1 != null)
                return;
            if (synchronizer1 != null && synchronizer2 != null)
            {
                var owners = synchronizer2.Owners;
                target.RemoveComponent(synchronizer2);
                target.AddComponent(synchronizer1);
                foreach (var owner in owners)
                    Synchronize(command, owner, priority);
                return;
            }

            if (synchronizer1 != null)
                target.AddComponent(synchronizer1);
            else if (synchronizer2 != null)
                command.AddComponent(synchronizer2);
            else
            {
                synchronizer1 = new SynchronizationCommandExecutorDecorator(priority);
                command.AddComponent(synchronizer1);
                target.AddComponent(synchronizer1);
            }
        }

        private static void AddRemoveForceExecuteCommands(IMetadataContext metadata, ICompositeCommand command, bool remove)
        {
            if (remove && !metadata.Contains(InternalMetadata.AllowForceExecuteCommands))
                return;

            metadata.AddOrUpdate(InternalMetadata.AllowForceExecuteCommands, command, (command, remove), (_, _, currentValue, s) =>
            {
                var editor = ItemOrListEditor<ICompositeCommand>.FromRawValue(currentValue);
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

        private static bool IsForceExecute(ICompositeCommand command, ICompositeCommand? executingCommand, IReadOnlyMetadataContext? metadata)
        {
            if (metadata != null && (ReferenceEquals(metadata, MugenExtensions.ForceExecuteMetadata) || metadata.TryGet(CommandMetadata.ForceExecute, out var value) && value))
                return true;

            if (command.HasMetadata && command.Metadata.TryGet(InternalMetadata.AllowForceExecuteCommands, out var commands))
            {
                if (executingCommand != null && !ReferenceEquals(command, executingCommand) &&
                    ItemOrIReadOnlyList.FromRawValue<ICompositeCommand>(commands).Contains(executingCommand))
                    return true;
            }

            return false;
        }

        private async Task<bool> ExecuteAsync(ICompositeCommand command, ItemOrArray<ICommandExecutorComponent> components, object? parameter,
            CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            CancellationTokenSource? cts = null;
            var oldValue = Interlocked.Exchange(ref _executingCommand, command);
            try
            {
                if (oldValue == null)
                    RaiseCanExecuteChanged(metadata);
                else if (!IsForceExecute(command, oldValue, metadata))
                    return false;

                cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, default);
                Interlocked.Exchange(ref _cancellationTokenSource, cts)?.Cancel();
                return await components.TryExecuteAsync(command, parameter, cts.Token, metadata).ConfigureAwait(false);
            }
            finally
            {
                Interlocked.CompareExchange(ref _cancellationTokenSource, null, cts);
                cts?.Dispose();
                if (Interlocked.CompareExchange(ref _executingCommand, oldValue, command) == command && oldValue == null)
                    RaiseCanExecuteChanged(metadata);
            }
        }

        private void RaiseCanExecuteChanged(IReadOnlyMetadataContext? metadata)
        {
            foreach (var owner in Owners)
                owner.RaiseCanExecuteChanged(metadata);
        }

        private sealed class CommandExecutorInterceptor : ComponentDecoratorBase<ICompositeCommand, ICommandExecutorComponent>, ICommandExecutorComponent
        {
            private readonly SynchronizationCommandExecutorDecorator _synchronizer;

            public CommandExecutorInterceptor(SynchronizationCommandExecutorDecorator synchronizer, int priority)
                : base(priority)
            {
                _synchronizer = synchronizer;
            }

            public bool IsExecuting(ICompositeCommand command, IReadOnlyMetadataContext? metadata) =>
                _synchronizer._executingCommand != null || Components.IsExecuting(command, metadata);

            public Task<bool> TryExecuteAsync(ICompositeCommand command, object? parameter, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata) =>
                _synchronizer.ExecuteAsync(command, Components, parameter, cancellationToken, metadata);
        }
    }
}