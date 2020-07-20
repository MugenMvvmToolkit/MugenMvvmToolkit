using System;
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
            CommandExecutionMode = CommandExecutionMode.CanExecuteBeforeExecute;
            EventThreadMode = ThreadExecutionMode.Main;
        }

        #endregion

        #region Properties

        public bool AllowMultipleExecution { get; set; }

        public CommandExecutionMode CommandExecutionMode { get; set; }

        public ThreadExecutionMode EventThreadMode { get; set; }

        public int Priority { get; set; } = CommandComponentPriority.CommandProvider;

        #endregion

        #region Implementation of interfaces

        public ICompositeCommand? TryGetCommand<TParameter>(ICommandManager commandManager, object request, IReadOnlyMetadataContext? metadata)
        {
            if (request is Delegate execute)
            {
                var compositeCommand = new CompositeCommand();
                compositeCommand.AddComponent(new DelegateExecutorCommandComponent<TParameter>(execute, null, CommandExecutionMode, AllowMultipleExecution));
                return compositeCommand;
            }

            if (request is DelegateCommandRequest commandRequest)
            {
                var command = new CompositeCommand(metadata, _componentCollectionManager);
                command.AddComponent(new DelegateExecutorCommandComponent<TParameter>(commandRequest.Execute, commandRequest.CanExecute, commandRequest.ExecutionMode.GetValueOrDefault(CommandExecutionMode),
                    commandRequest.AllowMultipleExecution.GetValueOrDefault(AllowMultipleExecution)));
                if (commandRequest.CanExecute != null && commandRequest.Notifiers != null && commandRequest.Notifiers.Count > 0)
                    command.AddComponent(new ConditionEventCommandComponent(_threadDispatcher, commandRequest.EventThreadMode ?? EventThreadMode, commandRequest.Notifiers, commandRequest.CanNotify));
                return command;
            }

            return null;
        }

        #endregion
    }
}