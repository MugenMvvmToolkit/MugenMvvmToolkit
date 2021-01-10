using System;
using System.Collections.Generic;
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
using MugenMvvm.Internal;
using MugenMvvm.Metadata;

namespace MugenMvvm.Commands.Components
{
    public sealed class DelegateCommandProvider : ICommandProviderComponent, IHasPriority
    {
        #region Fields

        private readonly IComponentCollectionManager? _componentCollectionManager;
        private readonly IThreadDispatcher? _threadDispatcher;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public DelegateCommandProvider(IThreadDispatcher? threadDispatcher = null, IComponentCollectionManager? componentCollectionManager = null)
        {
            _componentCollectionManager = componentCollectionManager;
            _threadDispatcher = threadDispatcher;
            CommandExecutionBehavior = CommandExecutionBehavior.CheckCanExecute;
            EventThreadMode = ThreadExecutionMode.Main;
        }

        #endregion

        #region Properties

        public bool AllowMultipleExecution { get; set; }

        public CommandExecutionBehavior CommandExecutionBehavior { get; set; }

        public ThreadExecutionMode EventThreadMode { get; set; }

        public bool CacheCommandEventHandler { get; set; } = true;

        public int Priority { get; set; } = CommandComponentPriority.CommandProvider;

        #endregion

        #region Implementation of interfaces

        public ICompositeCommand? TryGetCommand<TParameter>(ICommandManager commandManager, object? owner, object request, IReadOnlyMetadataContext? metadata)
        {
            if (request is Delegate execute)
            {
                var compositeCommand = new CompositeCommand();
                compositeCommand.AddComponent(new DelegateCommandExecutor<TParameter>(execute, null, CommandExecutionBehavior, AllowMultipleExecution));
                return compositeCommand;
            }

            if (request is not DelegateCommandRequest commandRequest)
                return null;

            var command = new CompositeCommand(null, _componentCollectionManager);
            command.AddComponent(new DelegateCommandExecutor<TParameter>(commandRequest.Execute, commandRequest.CanExecute, commandRequest.ExecutionMode ?? CommandExecutionBehavior,
                commandRequest.AllowMultipleExecution.GetValueOrDefault(AllowMultipleExecution)));
            if (command.HasCanExecute(metadata))
                command.AddComponent(GetCommandEventHandler(owner, commandRequest, metadata), metadata);
            return command;
        }

        #endregion

        #region Methods

        private CommandEventHandler GetCommandEventHandler(object? owner, DelegateCommandRequest commandRequest, IReadOnlyMetadataContext? metadata)
        {
            if (CacheCommandEventHandler && commandRequest.CanNotify == null && commandRequest.Notifiers.Count == 0 && owner is IMetadataOwner<IMetadataContext> m)
                return m.Metadata.GetOrAdd(InternalMetadata.CommandEventHandler, (this, owner, commandRequest, metadata), (_, _, s) => s.Item1.GetCommandEventHandlerInternal(s.owner, s.commandRequest, s.metadata));

            return GetCommandEventHandlerInternal(owner, commandRequest, metadata);
        }

        private CommandEventHandler GetCommandEventHandlerInternal(object? owner, DelegateCommandRequest commandRequest, IReadOnlyMetadataContext? metadata)
        {
            var handler = new CommandEventHandler(_threadDispatcher, commandRequest.EventThreadMode ?? EventThreadMode) {CanNotify = commandRequest.CanNotify};
            ItemOrIEnumerable<object> notifiers = commandRequest.Notifiers.IsEmpty ? owner! : commandRequest.Notifiers;
            foreach (var notifier in notifiers)
                handler.AddNotifier(notifier, metadata);

            return handler;
        }

        #endregion
    }
}