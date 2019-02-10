using System;
using System.Collections.Generic;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Commands
{
    public interface IRelayCommandDispatcher
    {
        IExecutorRelayCommandMediator GetMediator<TParameter>(IRelayCommand relayCommand, Delegate execute, Delegate? canExecute,
            IReadOnlyCollection<object>? notifiers, IReadOnlyMetadataContext metadata);

        void AddMediatorFactory(Func<IRelayCommand, Delegate, Delegate, IReadOnlyCollection<object>?, IReadOnlyMetadataContext, IRelayCommandMediator?> mediatorFactory);//todo add interface

        void RemoveMediatorFactory(Func<IRelayCommand, Delegate, Delegate, IReadOnlyCollection<object>?, IReadOnlyMetadataContext, IRelayCommandMediator?> mediatorFactory);

        void ClearMediatorFactories();

        void CleanupCommands(object target, IReadOnlyMetadataContext metadata);
    }
}