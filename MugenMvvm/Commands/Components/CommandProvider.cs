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
        public CommandProvider(IThreadDispatcher? threadDispatcher = null, IComponentCollectionManager? componentCollectionManager = null,
            int priority = CommandComponentPriority.CommandProvider)
        {
            _componentCollectionManager = componentCollectionManager;
            _threadDispatcher = threadDispatcher;
            EventThreadMode = ThreadExecutionMode.Main;
            CacheCommandNotifier = true;
            Priority = priority;
        }

        public bool AllowMultipleExecution { get; set; }

        public ThreadExecutionMode EventThreadMode { get; set; }

        public bool CacheCommandNotifier { get; set; }

        public int Priority { get; init; }

        public ICompositeCommand TryGetCommand<TParameter>(ICommandManager commandManager, object? owner, object request, IReadOnlyMetadataContext? metadata)
        {
            var command = new CompositeCommand(null, _componentCollectionManager);
            command.AddComponent(new CommandEventHandler(_threadDispatcher, EventThreadMode));

            if (request is Delegate execute)
                DelegateCommandExecutor.Add<TParameter>(command, execute, null, AllowMultipleExecution);
            else if (request is DelegateCommandRequest commandRequest)
            {
                DelegateCommandExecutor.Add<TParameter>(command, commandRequest.Execute, commandRequest.CanExecute,
                    commandRequest.AllowMultipleExecution.GetValueOrDefault(AllowMultipleExecution));
                var notifier = GetCommandCommandNotifier(owner, commandRequest, metadata);
                if (notifier != null)
                    command.AddComponent(notifier, metadata);
            }

            return command;
        }

        private static PropertyChangedCommandObserver? GetCommandCommandNotifierInternal(object? owner, DelegateCommandRequest commandRequest, IReadOnlyMetadataContext? metadata)
        {
            var commandNotifier = commandRequest.CanNotify == null ? null : new PropertyChangedCommandObserver { CanNotify = commandRequest.CanNotify };
            var notifiers = commandRequest.Notifiers.IsEmpty ? ItemOrIEnumerable.FromItem(owner) : commandRequest.Notifiers;
            foreach (var notifier in notifiers)
            {
                if (notifier is INotifyPropertyChanged propertyChanged)
                    (commandNotifier ??= new PropertyChangedCommandObserver()).Add(propertyChanged);
            }

            return commandNotifier;
        }

        private PropertyChangedCommandObserver? GetCommandCommandNotifier(object? owner, DelegateCommandRequest commandRequest, IReadOnlyMetadataContext? metadata)
        {
            if (CacheCommandNotifier && commandRequest.CanNotify == null && commandRequest.Notifiers.Count == 0 &&
                owner is IMetadataOwner<IMetadataContext> m and INotifyPropertyChanged)
            {
                return m.Metadata.GetOrAdd(InternalMetadata.CommandNotifier, (owner, commandRequest, metadata),
                    (_, _, s) => GetCommandCommandNotifierInternal(s.owner, s.commandRequest, s.metadata)!);
            }

            return GetCommandCommandNotifierInternal(owner, commandRequest, metadata);
        }
    }
}