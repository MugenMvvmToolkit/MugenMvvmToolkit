using System;
using System.Collections.Generic;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Interfaces.Commands
{
    public interface IChildRelayCommandMediatorProvider : IHasPriority
    {
        IReadOnlyList<IRelayCommandMediator> GetMediators<TParameter>(IRelayCommandMediatorProvider provider, IRelayCommand relayCommand, Delegate execute, Delegate? canExecute,
            IReadOnlyCollection<object>? notifiers, IReadOnlyMetadataContext metadata);
    }
}