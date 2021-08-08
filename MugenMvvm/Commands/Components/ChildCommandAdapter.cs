using System;
using System.Collections.Generic;
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

namespace MugenMvvm.Commands.Components
{
    public class ChildCommandAdapter : AttachableComponentBase<ICompositeCommand>, ICommandConditionComponent, ICommandExecutorComponent, IHasPriority
    {
        private readonly CommandListener _listener;
        private bool _suppressExecute;
        private bool _canExecuteEmptyResult;

        public ChildCommandAdapter()
        {
            _listener = new CommandListener(this);
        }

        public Func<IReadOnlyList<ICompositeCommand>, object?, IReadOnlyMetadataContext?, bool>? CanExecuteHandler { get; set; }

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

        public bool ExecuteSequentially { get; set; }

        public int Priority { get; init; } = CommandComponentPriority.ChildCommandAdapter;

        // ReSharper disable once InconsistentlySynchronizedField
        protected List<ICompositeCommand> Commands => _listener;

        public bool Add(ICompositeCommand command)
        {
            Should.NotBeNull(command, nameof(command));
            lock (_listener)
            {
                if (_listener.Contains(command))
                    return false;
                _listener.Add(command);
                command.AddComponent(_listener);
            }

            RaiseCanExecuteChanged();
            return true;
        }

        public bool Contains(ICompositeCommand command)
        {
            Should.NotBeNull(command, nameof(command));
            lock (_listener)
            {
                return _listener.Contains(command);
            }
        }

        public bool Remove(ICompositeCommand command)
        {
            Should.NotBeNull(command, nameof(command));
            lock (_listener)
            {
                if (!_listener.Remove(command))
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

            lock (_listener)
            {
                return CanExecuteInternal(parameter, metadata);
            }
        }

        public virtual bool IsExecuting(ICompositeCommand command, IReadOnlyMetadataContext? metadata)
        {
            lock (_listener)
            {
                for (var i = 0; i < _listener.Count; i++)
                {
                    if (_listener[i].IsExecuting(metadata))
                        return true;
                }
            }

            return false;
        }

        public virtual async Task<bool> TryExecuteAsync(ICompositeCommand command, object? parameter, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            if (SuppressExecute)
                return false;

            if (ExecuteSequentially)
            {
                if (!CanExecute(command, parameter, metadata))
                    return false;

                var index = 0;
                while (true)
                {
                    Task<bool> task;
                    lock (_listener)
                    {
                        if (index > _listener.Count - 1)
                            return false;

                        task = _listener[index].ExecuteAsync(parameter, cancellationToken, metadata);
                        ++index;
                    }

                    if (await task.ConfigureAwait(false))
                        return true;
                }
            }

            var tasks = new ItemOrListEditor<Task<bool>>();
            var result = false;
            lock (_listener)
            {
                if (!CanExecuteInternal(parameter, metadata))
                    return false;

                for (var i = 0; i < _listener.Count; i++)
                {
                    var task = _listener[i].ExecuteAsync(parameter, cancellationToken, metadata);
                    if (!task.IsCompletedSuccessfully())
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
            var canExecuteHandler = CanExecuteHandler;
            if (canExecuteHandler != null)
                return canExecuteHandler(_listener, parameter, metadata);

            if (_listener.Count == 0)
                return CanExecuteEmptyResult;

            for (var i = 0; i < _listener.Count; i++)
            {
                if (!_listener[i].CanExecute(parameter, metadata))
                    return false;
            }

            return true;
        }

        private void RaiseCanExecuteChanged(IReadOnlyMetadataContext? metadata = null) => OwnerOptional?.RaiseCanExecuteChanged(metadata);

        private sealed class CommandListener : List<ICompositeCommand>, ICommandEventHandlerComponent
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