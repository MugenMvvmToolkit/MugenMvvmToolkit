using System;
using System.Collections.Generic;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Interfaces.Commands
{
    public interface IRelayCommandMediatorProviderListener : IListener
    {
        void OnMediatorCreated(IRelayCommandMediatorProvider mediatorProvider, IRelayCommand relayCommand, Delegate execute, Delegate? canExecute, IReadOnlyCollection<object>? notifiers,
            IReadOnlyMetadataContext metadata, IExecutorRelayCommandMediator mediator);
    }
}