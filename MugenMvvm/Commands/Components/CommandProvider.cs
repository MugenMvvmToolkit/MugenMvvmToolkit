using System;
using System.ComponentModel;
using MugenMvvm.Attributes;
using MugenMvvm.Collections;
using MugenMvvm.Constants;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Commands;
using MugenMvvm.Interfaces.Commands.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Metadata;

namespace MugenMvvm.Commands.Components
{
    public sealed class CommandProvider : ICommandProviderComponent, IHasPriority
    {
        private readonly IComponentCollectionManager? _componentCollectionManager;

        [Preserve(Conditional = true)]
        public CommandProvider(IComponentCollectionManager? componentCollectionManager = null, int priority = CommandComponentPriority.CommandProvider)
        {
            _componentCollectionManager = componentCollectionManager;
            CacheCommandNotifier = true;
            Priority = priority;
        }

        public bool AllowMultipleExecution { get; set; }

        public bool CacheCommandNotifier { get; set; }

        public int Priority { get; init; }

        public ICompositeCommand TryGetCommand<TParameter>(ICommandManager commandManager, object? owner, object request, IReadOnlyMetadataContext? metadata)
        {
            var command = new CompositeCommand(null, _componentCollectionManager);
            command.AddComponent(new CommandEventHandler());

            if (request is Delegate execute)
                command.AddComponent(new DelegateCommandExecutor<TParameter>(execute, null, AllowMultipleExecution));
            else if (request is DelegateCommandRequest commandRequest)
            {
                command.AddComponent(new DelegateCommandExecutor<TParameter>(commandRequest.Execute, commandRequest.CanExecute,
                    commandRequest.AllowMultipleExecution.GetValueOrDefault(AllowMultipleExecution)));
                var observer = GetCommandObserver(owner, commandRequest, metadata);
                if (observer != null)
                    command.AddComponent(observer, metadata);
            }

            return command;
        }

        private static PropertyChangedCommandObserver? GetCommandObserverInternal(object? owner, DelegateCommandRequest commandRequest, IReadOnlyMetadataContext? metadata)
        {
            var commandNotifier = commandRequest.CanNotify == null ? null : new PropertyChangedCommandObserver { CanNotify = commandRequest.CanNotify };
            var notifiers = commandRequest.Notifiers.IsEmpty ? ItemOrIReadOnlyCollection.FromItem(owner) : commandRequest.Notifiers;
            foreach (var notifier in notifiers)
            {
                if (notifier is INotifyPropertyChanged propertyChanged)
                    (commandNotifier ??= new PropertyChangedCommandObserver()).Add(propertyChanged);
            }

            return commandNotifier;
        }

        private PropertyChangedCommandObserver? GetCommandObserver(object? owner, DelegateCommandRequest commandRequest, IReadOnlyMetadataContext? metadata)
        {
            if (CacheCommandNotifier && commandRequest.CanNotify == null && commandRequest.Notifiers.Count == 0 &&
                owner is IMetadataOwner<IMetadataContext> m and INotifyPropertyChanged)
            {
                return m.Metadata.GetOrAdd(InternalMetadata.CommandNotifier, (owner, commandRequest, metadata),
                    (_, _, s) => GetCommandObserverInternal(s.owner, s.commandRequest, s.metadata)!);
            }

            return GetCommandObserverInternal(owner, commandRequest, metadata);
        }
    }
}