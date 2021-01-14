using System;
using MugenMvvm.Collections;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Busy.Components
{
    public interface IBusyManagerComponent : IComponent<IBusyManager>
    {
        IBusyToken? TryBeginBusy(IBusyManager busyManager, object? request, IReadOnlyMetadataContext? metadata);

        IBusyToken? TryGetToken<TState>(IBusyManager busyManager, Func<TState, IBusyToken, IReadOnlyMetadataContext?, bool> filter, TState state,
            IReadOnlyMetadataContext? metadata);

        ItemOrIReadOnlyList<IBusyToken> TryGetTokens(IBusyManager busyManager, IReadOnlyMetadataContext? metadata);
    }
}