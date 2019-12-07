using System.Windows.Input;
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
using MugenMvvm.Metadata;

namespace MugenMvvm.Commands.Components
{
    public sealed class CommandMediatorProviderComponent : ICommandMediatorProviderComponent, IHasPriority
    {
        #region Fields

        private readonly IComponentCollectionProvider? _componentCollectionProvider;
        private readonly IThreadDispatcher? _threadDispatcher;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public CommandMediatorProviderComponent(IThreadDispatcher? threadDispatcher = null, IComponentCollectionProvider? componentCollectionProvider = null)
        {
            _componentCollectionProvider = componentCollectionProvider;
            _threadDispatcher = threadDispatcher;
            CommandExecutionMode = CommandExecutionMode.CanExecuteBeforeExecute;
            EventThreadMode = ThreadExecutionMode.Main;
        }

        #endregion

        #region Properties

        public bool AllowMultipleExecution { get; set; }

        public CommandExecutionMode CommandExecutionMode { get; set; }

        public ThreadExecutionMode EventThreadMode { get; set; }

        public int Priority { get; set; } = CommandComponentPriority.MediatorProvider;

        #endregion

        #region Implementation of interfaces

        public ICommandMediator? TryGetCommandMediator(ICommand command, IReadOnlyMetadataContext metadata)
        {
            var executor = metadata.Get(MediatorCommandMetadata.Executor);
            if (executor == null)
                return null;

            var notifiers = metadata.Get(MediatorCommandMetadata.Notifiers);
            var executionMode = metadata.Get(MediatorCommandMetadata.ExecutionMode, CommandExecutionMode);
            var allowMultipleExecution = metadata.Get(MediatorCommandMetadata.AllowMultipleExecution, AllowMultipleExecution);
            var mediator = new CommandMediator(_componentCollectionProvider, executionMode, allowMultipleExecution);
            mediator.AddComponent(executor, metadata);
            if (notifiers != null && notifiers.Count != 0)
            {
                var ignoreProperties = metadata.Get(MediatorCommandMetadata.IgnoreProperties);
                var eventExecutionMode = metadata.Get(MediatorCommandMetadata.EventThreadMode, EventThreadMode);
                mediator.AddComponent(new ConditionEventCommandMediatorComponent(_threadDispatcher, notifiers, ignoreProperties, eventExecutionMode!, command), metadata);
            }

            return mediator;
        }

        #endregion
    }
}