using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Collections;
using MugenMvvm.Components;
using MugenMvvm.Constants;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Commands;
using MugenMvvm.Interfaces.Commands.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Models.Components;
using MugenMvvm.Internal;

namespace MugenMvvm.Commands.Components
{
    public class ChildCommandAdapter : AttachableComponentBase<ICompositeCommand>, ICommandConditionComponent, ICommandExecutorComponent, IHasPriority
    {
        private static readonly ImmutableHashSet<ICompositeCommand> Empty = ImmutableHashSet<ICompositeCommand>.Empty.WithComparer(Default.ReferenceEqualityComparer);

        private readonly CommandListener _listener;
        private ImmutableHashSet<ICompositeCommand> _commands;
        private bool _suppressExecute;
        private bool _canExecuteEmptyResult;

        public ChildCommandAdapter()
        {
            _listener = new CommandListener(this);
            _commands = Empty;
        }

        public Func<ImmutableHashSet<ICompositeCommand>, object?, IReadOnlyMetadataContext?, bool>? CanExecuteHandler { get; set; }

        public Func<ImmutableHashSet<ICompositeCommand>, object?, CancellationToken, IReadOnlyMetadataContext?, Task<bool?>>? ExecuteHandler { get; set; }

        public bool SuppressExecute
        {
            get => _suppressExecute;
            set
            {
                if (_suppressExecute == value)
                    return;
                _suppressExecute = value;
                RaiseCanExecuteChanged();
            }
        }

        public bool CanExecuteEmptyResult
        {
            get => _canExecuteEmptyResult;
            set
            {
                if (_canExecuteEmptyResult == value)
                    return;
                _canExecuteEmptyResult = value;
                RaiseCanExecuteChanged();
            }
        }

        public int Priority { get; init; } = CommandComponentPriority.ChildCommandAdapter;

        protected IReadOnlyCollection<ICompositeCommand> Commands => _commands;

        public bool Add(ICompositeCommand command)
        {
            Should.NotBeNull(command, nameof(command));
            if (OwnerOptional == command)
                return false;
            lock (_listener)
            {
                if (!MugenExtensions.Add(ref _commands, command))
                    return false;
                command.AddComponent(_listener);
            }

            RaiseCanExecuteChanged();
            return true;
        }

        public bool Contains(ICompositeCommand command)
        {
            Should.NotBeNull(command, nameof(command));
            return _commands.Contains(command);
        }

        public bool Remove(ICompositeCommand command)
        {
            Should.NotBeNull(command, nameof(command));
            lock (_listener)
            {
                if (!MugenExtensions.Remove(ref _commands, command))
                    return false;
                command.RemoveComponent(_listener);
            }

            RaiseCanExecuteChanged();
            return true;
        }

        public virtual bool CanExecute(ICompositeCommand command, object? parameter, IReadOnlyMetadataContext? metadata)
        {
            if (SuppressExecute)
                return true;
            return CanExecuteInternal(_commands, parameter, metadata);
        }

        public virtual bool IsExecuting(ICompositeCommand command, IReadOnlyMetadataContext? metadata)
        {
            foreach (var cmd in _commands)
            {
                if (cmd.IsExecuting(metadata))
                    return true;
            }

            return false;
        }

        public virtual async Task<bool?> TryExecuteAsync(ICompositeCommand command, object? parameter, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            if (SuppressExecute)
                return null;

            var commands = _commands;
            var executeHandler = ExecuteHandler;
            if (executeHandler != null)
                return await executeHandler(commands, parameter, cancellationToken, metadata).ConfigureAwait(false);

            var tasks = new ItemOrListEditor<Task<bool?>>();
            bool? result = null;
            foreach (var cmd in commands)
            {
                var task = cmd.ExecuteAsync(parameter, cancellationToken, metadata);
                if (!task.IsCompletedSuccessfully())
                    tasks.Add(task);
                else if (task.Result.HasValue && !result.GetValueOrDefault())
                    result = task.Result;
            }

            foreach (var task in tasks)
            {
                await task.ConfigureAwait(false);
                if (task.Result.HasValue && !result.GetValueOrDefault())
                    result = task.Result;
            }

            return result;
        }

        public Task TryWaitAsync(ICompositeCommand command, IReadOnlyMetadataContext? metadata)
        {
            var tasks = new ItemOrListEditor<Task>();
            foreach (var cmd in _commands)
            {
                var task = cmd.WaitAsync(metadata);
                if (!task.IsCompletedSuccessfully())
                    tasks.Add(task);
            }

            return tasks.WhenAll();
        }

        private bool CanExecuteInternal(ImmutableHashSet<ICompositeCommand> commands, object? parameter, IReadOnlyMetadataContext? metadata)
        {
            var canExecuteHandler = CanExecuteHandler;
            if (canExecuteHandler != null)
                return canExecuteHandler(commands, parameter, metadata);

            if (commands.Count == 0)
                return CanExecuteEmptyResult;

            foreach (var command in commands)
            {
                if (!command.CanExecute(parameter, metadata))
                    return false;
            }

            return true;
        }

        private void RaiseCanExecuteChanged(IReadOnlyMetadataContext? metadata = null) => OwnerOptional?.RaiseCanExecuteChanged(metadata);

        private sealed class CommandListener : ICommandEventHandlerComponent, IDisposableComponent<ICompositeCommand>
        {
            private readonly ChildCommandAdapter _adapter;

            public CommandListener(ChildCommandAdapter adapter)
            {
                _adapter = adapter;
            }

            public void AddCanExecuteChanged(ICompositeCommand command, EventHandler? handler, IReadOnlyMetadataContext? metadata)
            {
            }

            public void RemoveCanExecuteChanged(ICompositeCommand command, EventHandler? handler, IReadOnlyMetadataContext? metadata)
            {
            }

            public void RaiseCanExecuteChanged(ICompositeCommand command, IReadOnlyMetadataContext? metadata) => _adapter.RaiseCanExecuteChanged(metadata);

            public void OnDisposing(ICompositeCommand owner, IReadOnlyMetadataContext? metadata)
            {
            }

            public void OnDisposed(ICompositeCommand owner, IReadOnlyMetadataContext? metadata) => _adapter.Remove(owner);
        }
    }
}