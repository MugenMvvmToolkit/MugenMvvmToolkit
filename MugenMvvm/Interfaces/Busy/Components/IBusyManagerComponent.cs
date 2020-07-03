using System;
using System.Collections.Generic;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;

namespace MugenMvvm.Interfaces.Busy.Components
{
    public interface IBusyManagerComponent : IComponent<IBusyManager>, ISuspendable
    {
        IBusyToken? TryBeginBusy<TRequest>(IBusyManager busyManager, in TRequest request, IReadOnlyMetadataContext? metadata);

        IBusyToken? TryGetToken<TState>(IBusyManager busyManager, in TState state, Func<TState, IBusyToken, IReadOnlyMetadataContext?, bool> filter, IReadOnlyMetadataContext? metadata);

        ItemOrList<IBusyToken, IReadOnlyList<IBusyToken>> TryGetTokens(IBusyManager busyManager, IReadOnlyMetadataContext? metadata);
    }
}