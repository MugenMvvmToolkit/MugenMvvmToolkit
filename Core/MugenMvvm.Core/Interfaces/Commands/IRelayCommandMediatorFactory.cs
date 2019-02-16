using System;
using System.Collections.Generic;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Commands
{
    public interface IRelayCommandMediatorFactory
    {
        IRelayCommandMediator? GetMediator<TParameter>(IRelayCommandDispatcher dispatcher, IRelayCommand relayCommand, Delegate execute, Delegate? canExecute,
            IReadOnlyCollection<object>? notifiers, IReadOnlyMetadataContext metadata);
    }
}