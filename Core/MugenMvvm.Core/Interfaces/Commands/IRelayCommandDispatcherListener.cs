using System;
using System.Collections.Generic;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Interfaces.Commands
{
    public interface IRelayCommandDispatcherListener : IListener
    {
        void OnMediatorCreated(IRelayCommandDispatcher dispatcher, IRelayCommand relayCommand, Delegate execute, Delegate? canExecute, IReadOnlyCollection<object>? notifiers,
            IReadOnlyMetadataContext metadata, IExecutorRelayCommandMediator mediator);
    }
}