using System;
using System.Collections.Generic;
using MugenMvvm.Attributes;
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

            var command = new CompositeCommand(metadata, _componentCollectionManager);
            command.AddComponent(new DelegateCommandExecutor<TParameter>(commandRequest.Execute, commandRequest.CanExecute, commandRequest.ExecutionMode ?? CommandExecutionBehavior,
                commandRequest.AllowMultipleExecution.GetValueOrDefault(AllowMultipleExecution)));
            if (commandRequest.CanExecute != null)
            {
                var notifiers = commandRequest.Notifiers;
                if (notifiers.Count > 0)
                    command.AddComponent(GetCommandEventHandler(commandRequest, notifiers));
                else if (owner != null)
                {
                    if (owner is IMetadataOwner<IMetadataContext> ctx)
                        command.AddComponent(ctx.Metadata.GetOrAdd(InternalMetadata.CommandEventHandler, (this, commandRequest, owner), (_, _, s) => s.Item1.GetCommandEventHandler(s.commandRequest, s.owner)));
                    else
                        command.AddComponent(GetCommandEventHandler(commandRequest, owner));
                }
            }

            return command;
        }

        #endregion

        #region Methods

        private CommandEventHandler GetCommandEventHandler(DelegateCommandRequest commandRequest, ItemOrList<object, IReadOnlyList<object>> notifiers) =>
            new(_threadDispatcher, commandRequest.EventThreadMode ?? EventThreadMode, notifiers, commandRequest.CanNotify);

        #endregion
    }
}