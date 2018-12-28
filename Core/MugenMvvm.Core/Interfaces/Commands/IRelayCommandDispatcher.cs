using System;
using System.Collections.Generic;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Commands
{
    public interface IRelayCommandDispatcher
    {
        IRelayCommandMediator GetMediator(Delegate execute, Delegate? canExecute, IReadOnlyCollection<object>? notifiers, IReadOnlyMetadataContext context);

        void CleanupCommands(object target, IReadOnlyMetadataContext context);
    }
}