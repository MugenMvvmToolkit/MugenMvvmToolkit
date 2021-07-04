using System;
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
using MugenMvvm.Internal;

namespace MugenMvvm.Commands.Components
{
    public sealed class ChildCommandAdapter : MultiAttachableComponentBase<ICompositeCommand>, ICommandConditionComponent, ICommandExecutorComponent, IHasPriority
    {
        private readonly CommandListener _commands;
        private bool _suppressExecute;
        private bool _canExecuteEmptyResult;
        private bool _canExecuteIfAnyCanExecute;

        public ChildCommandAdapter()
        {
            _commands = new CommandListener(this);
        }

        public Func<ActionToken>? ExecutionHandler { get; set; }

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

        public bool CanExecuteIfAnyCanExecute
        {
            get => _canExecuteIfAnyCanExecute;
            set
            {
                if (_canExecuteIfAnyCanExecute == value)
                    return;
                _canExecuteIfAnyCanExecute = value;
                RaiseCanExecuteChanged();
            }
        }

        public int Priority { get; set; } = CommandComponentPriority.ChildCommandAdapter;

        public void Add(ICompositeCommand command)
        {
            Should.NotBeNull(command, nameof(command));
            lock (_commands)
            {
                if (_commands.Contains(command))
                    return;
                _commands.Add(command);
                command.AddComponent(_commands);
            }

            RaiseCanExecuteChanged();
        }

        public bool Contains(ICompositeCommand command)
        {
            Should.NotBeNull(command, nameof(command));
            lock (_commands)
            {
                return _commands.Contains(command);
            }
        }

        public void Remove(ICompositeCommand command)
        {
            Should.NotBeNull(command, nameof(command));
            lock (_commands)
            {
                if (!_commands.Remove(command))
                    return;
                command.RemoveComponent(_commands);
            }

            RaiseCanExecuteChanged();
        }

        public bool CanExecute(ICompositeCommand command, object? parameter, IReadOnlyMetadataContext? metadata)
        {
            if (SuppressExecute)
                return true;

            lock (_commands)
            {
                return CanExecuteInternal(parameter, metadata);
            }
        }

        public async ValueTask<bool> TryExecuteAsync(ICompositeCommand command, object? parameter, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            if (SuppressExecute)
                return false;

            using var t = ExecutionHandler?.Invoke();
            var tasks = new ItemOrListEditor<ValueTask<bool>>();
            var result = false;
            lock (_commands)
            {
                if (!CanExecuteInternal(parameter, metadata))
                    return false;

                var items = _commands.Items;
                for (var i = 0; i < _commands.Count; i++)
                {
                    var task = items[i].ExecuteAsync(parameter, cancellationToken, metadata);
                    if (!task.IsCompletedSuccessfully)
                        tasks.Add(task);
                    else if (task.Result)
                        result = true;
                }
            }

            foreach (var task in tasks)
            {
                await task.ConfigureAwait(false);
                if (task.Result)
                    result = true;
            }

            return result;
        }

        private bool CanExecuteInternal(object? parameter, IReadOnlyMetadataContext? metadata)
        {
            if (_commands.Count == 0)
                return CanExecuteEmptyResult;

            var items = _commands.Items;
            if (CanExecuteIfAnyCanExecute)
            {
                for (var i = 0; i < _commands.Count; i++)
                {
                    if (items[i].CanExecute(parameter, metadata))
                        return true;
                }

                return false;
            }

            for (var i = 0; i < _commands.Count; i++)
            {
                if (!items[i].CanExecute(parameter, metadata))
                    return false;
            }

            return true;
        }

        private void RaiseCanExecuteChanged(IReadOnlyMetadataContext? metadata = null)
        {
            foreach (var owner in Owners)
                owner.RaiseCanExecuteChanged(metadata);
        }

        private sealed class CommandListener : ListInternal<ICompositeCommand>, ICommandEventHandlerComponent
        {
            private readonly ChildCommandAdapter _adapter;

            public CommandListener(ChildCommandAdapter adapter) : base(2)
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
        }
    }
}