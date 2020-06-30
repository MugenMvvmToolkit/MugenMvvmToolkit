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
    public sealed class DelegateCommandProvider : ICommandProviderComponent, IHasPriority, DelegateCommandRequest.IProvider
    {
        #region Fields

        private readonly IComponentCollectionManager? _componentCollectionManager;
        private readonly IMetadataContextManager? _metadataContextManager;
        private readonly IThreadDispatcher? _threadDispatcher;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public DelegateCommandProvider(IThreadDispatcher? threadDispatcher = null, IComponentCollectionManager? componentCollectionManager = null, IMetadataContextManager? metadataContextManager = null)
        {
            _componentCollectionManager = componentCollectionManager;
            _metadataContextManager = metadataContextManager;
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

        public ICompositeCommand? TryGetCommand<TRequest>(in TRequest request, IReadOnlyMetadataContext? metadata)
        {
            if (typeof(TRequest) == typeof(DelegateCommandRequest))
                return MugenExtensions.CastGeneric<TRequest, DelegateCommandRequest>(request).TryGetCommand(this, metadata);
            return null;
        }

        ICompositeCommand? DelegateCommandRequest.IProvider.TryGetCommand<T>(in DelegateCommandRequest request, IReadOnlyMetadataContext? metadata)
        {
            var command = new CompositeCommand(metadata, _componentCollectionManager, _metadataContextManager);
            command.AddComponent(new DelegateExecutorCommandComponent<T>(request.Execute, request.CanExecute, request.ExecutionMode.GetValueOrDefault(CommandExecutionMode),
                request.AllowMultipleExecution.GetValueOrDefault(AllowMultipleExecution)));
            if (request.CanExecute != null && request.Notifiers != null && request.Notifiers.Count > 0)
                command.AddComponent(new ConditionEventCommandComponent(_threadDispatcher, request.EventThreadMode ?? EventThreadMode, request.Notifiers, request.CanNotify));
            return command;
        }

        #endregion
    }
}