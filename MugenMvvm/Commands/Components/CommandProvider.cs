using System;
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

        public bool CacheCommandEventHandler { get; set; } = true;

        public int Priority { get; set; } = CommandComponentPriority.CommandProvider;

        public ICompositeCommand? TryGetCommand<TParameter>(ICommandManager commandManager, object? owner, object request, IReadOnlyMetadataContext? metadata)
        {
            if (request is RawCommandRequest)
            {
                var rawCommand = new CompositeCommand(null, _componentCollectionManager);
                rawCommand.AddComponent(GetCommandEventHandler(owner, null, metadata), metadata);
                return rawCommand;
            }

            if (request is Delegate execute)
            {
                var compositeCommand = new CompositeCommand(null, _componentCollectionManager);
                compositeCommand.AddComponent(new DelegateCommandExecutor<TParameter>(execute, null, CommandExecutionBehavior, AllowMultipleExecution));
                compositeCommand.AddComponent(GetCommandEventHandler(owner, null, metadata), metadata);
                return compositeCommand;
            }

            if (request is not DelegateCommandRequest commandRequest)
                return null;

            var command = new CompositeCommand(null, _componentCollectionManager);
            command.AddComponent(new DelegateCommandExecutor<TParameter>(commandRequest.Execute, commandRequest.CanExecute,
                commandRequest.ExecutionMode ?? CommandExecutionBehavior,
                commandRequest.AllowMultipleExecution.GetValueOrDefault(AllowMultipleExecution)));
            command.AddComponent(GetCommandEventHandler(owner, commandRequest, metadata), metadata);
            return command;
        }

        private CommandEventHandler GetCommandEventHandler(object? owner, DelegateCommandRequest? commandRequest, IReadOnlyMetadataContext? metadata)
        {
            if (CacheCommandEventHandler && (commandRequest == null || commandRequest.CanNotify == null && commandRequest.Notifiers.Count == 0) &&
                owner is IMetadataOwner<IMetadataContext> m)
            {
                return m.Metadata.GetOrAdd(InternalMetadata.CommandEventHandler, (this, owner, commandRequest, metadata),
                    (_, _, s) => s.Item1.GetCommandEventHandlerInternal(s.owner, s.commandRequest, s.metadata));
            }

            return GetCommandEventHandlerInternal(owner, commandRequest, metadata);
        }

        private CommandEventHandler GetCommandEventHandlerInternal(object? owner, DelegateCommandRequest? commandRequest, IReadOnlyMetadataContext? metadata)
        {
            var handler = new CommandEventHandler(_threadDispatcher, commandRequest?.EventThreadMode ?? EventThreadMode) {CanNotify = commandRequest?.CanNotify};
            ItemOrIEnumerable<object> notifiers = commandRequest == null || commandRequest.Notifiers.IsEmpty ? owner! : commandRequest.Notifiers;
            foreach (var notifier in notifiers)
                handler.AddNotifier(notifier, metadata);

            return handler;
        }
    }
}