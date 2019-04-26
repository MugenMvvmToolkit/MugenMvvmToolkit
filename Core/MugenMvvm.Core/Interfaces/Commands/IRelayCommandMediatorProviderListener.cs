using System;
using System.Collections.Generic;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Interfaces.Commands
{
    public interface IRelayCommandMediatorProviderListener : IListener
    {
        void OnMediatorCreated<TParameter>(IRelayCommandMediatorProvider mediatorProvider, IExecutorRelayCommandMediator mediator, IRelayCommand relayCommand, Delegate execute,
            Delegate? canExecute, IReadOnlyCollection<object>? notifiers, IReadOnlyMetadataContext metadata);
    }
}