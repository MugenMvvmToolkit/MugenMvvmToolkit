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
    public sealed class DelegateCommandProviderComponent : ICommandProviderComponent, IHasPriority, DelegateCommandRequest.IProvider
    {
        #region Fields

        private readonly IComponentCollectionProvider? _componentCollectionProvider;
        private readonly IThreadDispatcher? _threadDispatcher;
        private readonly FuncIn<DelegateCommandRequest, ICompositeCommand?> _tryGetCommandDelegate;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public DelegateCommandProviderComponent(IThreadDispatcher? threadDispatcher = null, IComponentCollectionProvider? componentCollectionProvider = null)
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
            if (_tryGetCommandDelegate is FuncIn<TRequest, ICompositeCommand> func)
                return func(request);
            return null;
        }

        ICompositeCommand? DelegateCommandRequest.IProvider.TryGetCommand<T>(in DelegateCommandRequest request, IReadOnlyMetadataContext? metadata)
        {
            var command = new CompositeCommand(_componentCollectionProvider);
            command.AddComponent(new DelegateExecutorCommandComponent<T>(request.Execute, request.CanExecute, request.ExecutionMode.GetValueOrDefault(CommandExecutionMode),
                request.AllowMultipleExecution.GetValueOrDefault(AllowMultipleExecution)));
            if (request.CanExecute != null && request.Notifiers != null && request.Notifiers.Count > 0)
                command.AddComponent(new ConditionEventCommandComponent(_threadDispatcher, request.EventThreadMode ?? EventThreadMode, request.Notifiers, request.CanNotify));
            return command;
        }

        #endregion

        #region Methods

        private ICompositeCommand? TryGetCommand(in DelegateCommandRequest request)
        {
            return request.TryGetCommand(this, null);
        }

        #endregion
    }
}