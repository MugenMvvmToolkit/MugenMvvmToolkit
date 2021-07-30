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
        private readonly CommandListener _listener;
        private ListInternal<ICompositeCommand> _commands;
        private bool _suppressExecute;
        private bool _canExecuteEmptyResult;
        private bool _canExecuteIfAnyCanExecute;

        public ChildCommandAdapter()
        {
            _listener = new CommandListener(this);
            _commands = new ListInternal<ICompositeCommand>(2);
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

        public bool ExecuteSequentially { get; set; }

        public int Priority { get; init; } = CommandComponentPriority.ChildCommandAdapter;

        public void Add(ICompositeCommand command)
        {
            Should.NotBeNull(command, nameof(command));
            lock (_listener)
            {
                if (_commands.Contains(command))
                    return;
                _commands.Add(command);
                command.AddComponent(_listener);
            }

            RaiseCanExecuteChanged();
        }

        public bool Contains(ICompositeCommand command)
        {
            Should.NotBeNull(command, nameof(command));
            lock (_listener)
            {
                return _commands.Contains(command);
            }
        }

        public void Remove(ICompositeCommand command)
        {
            Should.NotBeNull(command, nameof(command));
            lock (_listener)
            {
                if (!_commands.Remove(command))
                    return;
                command.RemoveComponent(_listener);
            }

            RaiseCanExecuteChanged();
        }

        public bool CanExecute(ICompositeCommand command, object? parameter, IReadOnlyMetadataContext? metadata)
        {
            if (SuppressExecute)
                return true;

            lock (_listener)
            {
                return CanExecuteInternal(parameter, metadata);
            }
        }

        public async ValueTask<bool> TryExecuteAsync(ICompositeCommand command, object? parameter, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            using var t = ExecutionHandler?.Invoke();
            if (ExecuteSequentially)
            {
                if (!CanExecute(command, parameter, metadata))
                    return false;

                var index = 0;
                while (true)
                {
                    ValueTask<bool> task;
                    lock (_listener)
                    {
                        if (index > _commands.Count - 1)
                            return false;

                        task = _commands.Items[index].ExecuteAsync(parameter, cancellationToken, metadata);
                        ++index;
                    }

                    if (await task.ConfigureAwait(false))
                        return true;
                }
            }

            if (SuppressExecute)
                return false;
            var tasks = new ItemOrListEditor<ValueTask<bool>>();
            var result = false;
            lock (_listener)
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

        private sealed class CommandListener : ICommandEventHandlerComponent
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
        }
    }
}