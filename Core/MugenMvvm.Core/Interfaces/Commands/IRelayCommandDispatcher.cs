using System;
using System.Collections.Generic;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Interfaces.Commands
{
    public interface IRelayCommandDispatcher : IHasListeners<IRelayCommandDispatcherListener>
    {
        IExecutorRelayCommandMediatorFactory ExecutorMediatorFactory { get; set; }

        void AddMediatorFactory(IRelayCommandMediatorFactory factory);

        void RemoveMediatorFactory(IRelayCommandMediatorFactory factory);

        IReadOnlyList<IRelayCommandMediatorFactory> GetMediatorFactories();

        IExecutorRelayCommandMediator GetExecutorMediator<TParameter>(IRelayCommand relayCommand, Delegate execute, Delegate? canExecute,
            IReadOnlyCollection<object>? notifiers, IReadOnlyMetadataContext metadata);
    }
}