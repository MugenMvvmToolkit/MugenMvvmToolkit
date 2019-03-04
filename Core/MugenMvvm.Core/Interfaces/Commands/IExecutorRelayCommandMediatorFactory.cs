using System;
using System.Collections.Generic;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Commands
{
    public interface IExecutorRelayCommandMediatorFactory
    {
        void Initialize(IRelayCommandDispatcher dispatcher);

        IExecutorRelayCommandMediator GetExecutorMediator<TParameter>(IRelayCommand relayCommand, IReadOnlyList<IRelayCommandMediator> mediators, Delegate execute,
            Delegate? canExecute, IReadOnlyCollection<object>? notifiers, IReadOnlyMetadataContext metadata);
    }
}