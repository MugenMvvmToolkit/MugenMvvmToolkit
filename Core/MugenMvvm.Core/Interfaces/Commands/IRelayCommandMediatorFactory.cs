using System;
using System.Collections.Generic;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Interfaces.Commands
{
    public interface IRelayCommandMediatorFactory : IHasPriority
    {
        IReadOnlyList<IRelayCommandMediator> GetMediators<TParameter>(IRelayCommandMediatorProvider mediatorProvider, IRelayCommand relayCommand, Delegate execute, Delegate? canExecute,
            IReadOnlyCollection<object>? notifiers, IReadOnlyMetadataContext metadata);
    }
}