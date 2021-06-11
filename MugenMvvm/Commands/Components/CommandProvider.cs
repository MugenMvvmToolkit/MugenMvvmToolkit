using System;
using System.ComponentModel;
using MugenMvvm.Attributes;
using MugenMvvm.Collections;
using MugenMvvm.Constants;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Commands;
using MugenMvvm.Interfaces.Commands.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Threading;
using MugenMvvm.Metadata;

namespace MugenMvvm.Commands.Components
{
    public sealed class CommandProvider : ICommandProviderComponent, IHasPriority
    {
        private readonly IComponentCollectionManager? _componentCollectionManager;
        private readonly IThreadDispatcher? _threadDispatcher;

        [Preserve(Conditional = true)]
        public CommandProvider(IThreadDispatcher? threadDispatcher = null, IComponentCollectionManager? componentCollectionManager = null)
        {
            _componentCollectionManager = componentCollectionManager;
            _threadDispatcher = threadDispatcher;
        }

        public bool AllowMultipleExecution { get; set; }

        public CommandExecutionBehavior CommandExecutionBehavior { get; set; } = CommandExecutionBehavior.CheckCanExecute;

        public ThreadExecutionMode EventThreadMode { get; set; } = ThreadExecutionMode.Main;

        public bool CacheCommandNotifier { get; set; } = true;

        public int Priority { get; set; } = CommandComponentPriority.CommandProvider;

        private static PropertyChangedCommandNotifier GetCommandCommandNotifierInternal(object? owner, DelegateCommandRequest commandRequest, IReadOnlyMetadataContext? metadata)
        {
            var commandNotifier = new PropertyChangedCommandNotifier { CanNotify = commandRequest.CanNotify };
            var notifiers = commandRequest.Notifiers.IsEmpty ? ItemOrIEnumerable.FromItem(owner) : commandRequest.Notifiers;
            foreach (var notifier in notifiers)
            {
                if (notifier is INotifyPropertyChanged propertyChanged)
                    commandNotifier.AddNotifier(propertyChanged, metadata);
            }

            return commandNotifier;
        }

        public ICompositeCommand TryGetCommand<TParameter>(ICommandManager commandManager, object? owner, object request, IReadOnlyMetadataContext? metadata)
        {
            var command = new CompositeCommand(null, _componentCollectionManager);
            command.AddComponent(new CommandEventHandler(_threadDispatcher, EventThreadMode));

            if (request is Delegate execute)
                command.AddComponent(new DelegateCommandExecutor<TParameter>(execute, null, CommandExecutionBehavior, AllowMultipleExecution));
            else if (request is DelegateCommandRequest commandRequest)
            {
                command.AddComponent(new DelegateCommandExecutor<TParameter>(commandRequest.Execute, commandRequest.CanExecute,
                    commandRequest.ExecutionMode ?? CommandExecutionBehavior,
                    commandRequest.AllowMultipleExecution.GetValueOrDefault(AllowMultipleExecution)));
                command.AddComponent(GetCommandCommandNotifier(owner, commandRequest, metadata), metadata);
            }

            return command;
        }

        private PropertyChangedCommandNotifier GetCommandCommandNotifier(object? owner, DelegateCommandRequest commandRequest, IReadOnlyMetadataContext? metadata)
        {
            if (CacheCommandNotifier && commandRequest.CanNotify == null && commandRequest.Notifiers.Count == 0 && owner is IMetadataOwner<IMetadataContext> m)
            {
                return m.Metadata.GetOrAdd(InternalMetadata.CommandNotifier, (owner, commandRequest, metadata),
                    (_, _, s) => GetCommandCommandNotifierInternal(s.owner, s.commandRequest, s.metadata));
            }

            return GetCommandCommandNotifierInternal(owner, commandRequest, metadata);
        }
    }
}