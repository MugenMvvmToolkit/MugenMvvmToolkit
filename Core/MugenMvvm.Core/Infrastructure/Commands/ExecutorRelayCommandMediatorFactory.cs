using System;
using System.Collections.Generic;
using MugenMvvm.Attributes;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Commands;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Threading;
using MugenMvvm.Metadata;

namespace MugenMvvm.Infrastructure.Commands
{
    public class ExecutorRelayCommandMediatorFactory : IExecutorRelayCommandMediatorFactory, IRelayCommandMediatorFactory
    {
        #region Constructors

        [Preserve(Conditional = true)]
        public ExecutorRelayCommandMediatorFactory(IThreadDispatcher threadDispatcher)
        {
            Should.NotBeNull(threadDispatcher, nameof(threadDispatcher));
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

        #endregion

        #region Implementation of interfaces

        public IExecutorRelayCommandMediator GetExecutorMediator<TParameter>(IRelayCommandDispatcher dispatcher, IRelayCommand relayCommand,
            IReadOnlyList<IRelayCommandMediator> mediators, Delegate execute,
            Delegate? canExecute, IReadOnlyCollection<object>? notifiers, IReadOnlyMetadataContext metadata)
        {
            return new ExecutorRelayCommandMediator<TParameter>(execute, canExecute, metadata.Get(RelayCommandMetadata.ExecutionMode, CommandExecutionMode),
                metadata.Get(RelayCommandMetadata.AllowMultipleExecution, AllowMultipleExecution), mediators);
        }

        public IRelayCommandMediator? GetMediator<TParameter>(IRelayCommandDispatcher dispatcher, IRelayCommand relayCommand, Delegate execute, Delegate? canExecute,
            IReadOnlyCollection<object>? notifiers, IReadOnlyMetadataContext metadata)
        {
            if (notifiers == null || notifiers.Count == 0)
                return null;
            return new ConditionEventRelayCommandMediator(ThreadDispatcher, notifiers, metadata.Get(RelayCommandMetadata.IgnoreProperties),
                metadata.Get(RelayCommandMetadata.EventThreadMode, EventThreadMode), relayCommand);
        }

        #endregion
    }
}