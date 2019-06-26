using System;
using System.Collections.Generic;
using System.Windows.Input;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Commands.Components
{
    public interface ICommandMediatorProviderListener : IComponent<ICommandMediatorProvider>
    {
        void OnCommandMediatorCreated<TParameter>(ICommandMediatorProvider provider, ICommandMediator mediator, ICommand command, Delegate execute,
            Delegate? canExecute, IReadOnlyCollection<object>? notifiers, IReadOnlyMetadataContext metadata);
    }
}