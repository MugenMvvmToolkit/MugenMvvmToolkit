using System;
using System.Collections.Generic;
using MugenMvvm.Interfaces.Commands.Mediators;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Commands
{
    public interface IRelayCommandDispatcherListener
    {
        void OnMediatorCreated(IRelayCommandDispatcher dispatcher, IRelayCommand relayCommand, Delegate execute, Delegate? canExecute, IReadOnlyCollection<object>? notifiers,
            IReadOnlyMetadataContext metadata, IExecutorRelayCommandMediator mediator);
    }
}