using System;
using System.Collections.Generic;
using System.Windows.Input;
using MugenMvvm.Attributes;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Commands.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Threading;
using MugenMvvm.Metadata;

namespace MugenMvvm.Infrastructure.Commands.Components
{
    public sealed class CommandMediatorProviderComponent : ICommandMediatorProviderComponent
    {
        #region Fields

        private readonly IComponentCollectionProvider _componentCollectionProvider;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public CommandMediatorProviderComponent(IThreadDispatcher threadDispatcher, IComponentCollectionProvider componentCollectionProvider)
        {
            Should.NotBeNull(threadDispatcher, nameof(threadDispatcher));
            Should.NotBeNull(componentCollectionProvider, nameof(componentCollectionProvider));
            _componentCollectionProvider = componentCollectionProvider;
            ThreadDispatcher = threadDispatcher;
            CommandExecutionMode = CommandExecutionMode.CanExecuteBeforeExecute;
            EventThreadMode = ThreadExecutionMode.Main;
        }

        #endregion

        #region Properties

        public IThreadDispatcher ThreadDispatcher { get; }

        public bool AllowMultipleExecution { get; set; }

        public CommandExecutionMode CommandExecutionMode { get; set; }

        public ThreadExecutionMode EventThreadMode { get; set; }

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        public int GetPriority(object source)
        {
            return Priority;
        }

        public ICommandMediator TryGetCommandMediator<TParameter>(ICommand command, Delegate execute, Delegate canExecute, IReadOnlyCollection<object> notifiers, IReadOnlyMetadataContext metadata)
        {
            var mediator = new CommandMediator<TParameter>(_componentCollectionProvider, execute, canExecute,
                metadata.Get(RelayCommandMetadata.ExecutionMode, CommandExecutionMode),
                metadata.Get(RelayCommandMetadata.AllowMultipleExecution, AllowMultipleExecution));
            if (notifiers != null && notifiers.Count != 0)
            {
                mediator.AddComponent(new ConditionEventCommandMediatorComponent(ThreadDispatcher, notifiers, metadata.Get(RelayCommandMetadata.IgnoreProperties),
                    metadata.Get(RelayCommandMetadata.EventThreadMode, EventThreadMode)!, command));
            }

            return mediator;
        }

        #endregion
    }
}