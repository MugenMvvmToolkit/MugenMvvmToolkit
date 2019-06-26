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

        public ICommandMediator? TryGetCommandMediator<TParameter>(ICommand command, IReadOnlyMetadataContext metadata)
        {
            var execute = metadata.Get(MediatorCommandMetadata.Execute);
            if (!DelegateCommandMediator<TParameter>.IsSupported(execute))
                return null;

            var canExecute = metadata.Get(MediatorCommandMetadata.CanExecute);
            if (!DelegateCommandMediator<TParameter>.IsCanExecuteSupported(canExecute))
                return null;

            var notifiers = metadata.Get(MediatorCommandMetadata.Notifiers);
            var executionMode = metadata.Get(MediatorCommandMetadata.ExecutionMode, CommandExecutionMode);
            var allowMultipleExecution = metadata.Get(MediatorCommandMetadata.AllowMultipleExecution, AllowMultipleExecution);

            var mediator = new DelegateCommandMediator<TParameter>(_componentCollectionProvider, execute!, canExecute, executionMode, allowMultipleExecution);
            if (notifiers != null && notifiers.Count != 0)
            {
                var ignoreProperties = metadata.Get(MediatorCommandMetadata.IgnoreProperties);
                var eventExecutionMode = metadata.Get(MediatorCommandMetadata.EventThreadMode, EventThreadMode);
                mediator.AddComponent(new ConditionEventCommandMediatorComponent(ThreadDispatcher, notifiers, ignoreProperties, eventExecutionMode!, command));
            }

            return mediator;
        }

        #endregion
    }
}