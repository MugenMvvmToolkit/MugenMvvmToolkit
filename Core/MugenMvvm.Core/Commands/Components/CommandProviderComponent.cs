using MugenMvvm.Attributes;
using MugenMvvm.Constants;
using MugenMvvm.Delegates;
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
    public sealed class CommandProviderComponent : ICommandProviderComponent, IHasPriority
    {
        #region Fields

        private readonly IComponentCollectionProvider? _componentCollectionProvider;
        private readonly IThreadDispatcher? _threadDispatcher;
        private readonly FuncEx<CommandRequest, ICompositeCommand?> _tryGetCommandDelegate;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public CommandProviderComponent(IThreadDispatcher? threadDispatcher = null, IComponentCollectionProvider? componentCollectionProvider = null)
        {
            _componentCollectionProvider = componentCollectionProvider;
            _threadDispatcher = threadDispatcher;
            CommandExecutionMode = CommandExecutionMode.CanExecuteBeforeExecute;
            EventThreadMode = ThreadExecutionMode.Main;
            _tryGetCommandDelegate = TryGetCommand;
        }

        #endregion

        #region Properties

        public bool AllowMultipleExecution { get; set; }

        public CommandExecutionMode CommandExecutionMode { get; set; }

        public ThreadExecutionMode EventThreadMode { get; set; }

        public int Priority { get; set; } = CommandComponentPriority.CommandProvider;

        #endregion

        #region Implementation of interfaces

        public ICompositeCommand? TryGetCommand<TRequest>(in TRequest request, IReadOnlyMetadataContext? metadata)
        {
            if (_tryGetCommandDelegate is FuncEx<TRequest, ICompositeCommand> func)
                return func(request);
            return null;
        }

        #endregion

        #region Methods

        private ICompositeCommand? TryGetCommand(in CommandRequest request)
        {
            if (request.IsEmpty)
                return null;

            var command = new CompositeCommand(_componentCollectionProvider);
            command.AddComponent(request.Executor);
            if (command.HasCanExecute)
            {
                var component = ExecutionModeCommandComponent.Get(request.ExecutionMode.GetValueOrDefault(CommandExecutionMode));
                if (component != null)
                    command.AddComponent(component);

                if (request.Notifiers != null && request.Notifiers.Count > 0)
                    command.AddComponent(new ConditionEventCommandComponent(_threadDispatcher, request.EventThreadMode ?? EventThreadMode, request.Notifiers, request.CanNotify));
            }
            if (!request.AllowMultipleExecution.GetValueOrDefault(AllowMultipleExecution))
                command.AddComponent(new DisableMultipleExecutionCommandComponent());
            return command;
        }

        #endregion
    }
}