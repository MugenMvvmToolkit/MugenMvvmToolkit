using System;
using System.Collections.Generic;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Commands
{
    public interface IExecutorChildRelayCommandMediatorProvider : IChildRelayCommandMediatorProvider
    {
        IExecutorRelayCommandMediator? TryGetExecutorMediator<TParameter>(IRelayCommandMediatorProvider provider, IRelayCommand relayCommand,
            IReadOnlyList<IRelayCommandMediator> mediators, Delegate execute,
            Delegate? canExecute, IReadOnlyCollection<object>? notifiers, IReadOnlyMetadataContext metadata);
    }
}