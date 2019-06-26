using System;
using System.Collections.Generic;
using System.Windows.Input;
using MugenMvvm.Interfaces.Commands.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Commands
{
    public interface ICommandMediatorProvider : IComponentOwner<ICommandMediatorProvider>
    {
        ICommandMediator GetCommandMediator<TParameter>(ICommand command, Delegate execute, Delegate? canExecute,
            IReadOnlyCollection<object>? notifiers, IReadOnlyMetadataContext metadata);
    }
}