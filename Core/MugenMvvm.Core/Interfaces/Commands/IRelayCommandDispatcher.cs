using System;
using System.Collections.Generic;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Interfaces.Commands
{
    public interface IRelayCommandDispatcher : IHasListeners<IRelayCommandDispatcherListener>
    {
        IExecutorRelayCommandMediatorFactory ExecutorMediatorFactory { get; }

        IComponentCollection<IRelayCommandMediatorFactory> MediatorFactories { get; }

        IExecutorRelayCommandMediator GetExecutorMediator<TParameter>(IRelayCommand relayCommand, Delegate execute, Delegate? canExecute,
            IReadOnlyCollection<object>? notifiers, IReadOnlyMetadataContext metadata);
    }
}