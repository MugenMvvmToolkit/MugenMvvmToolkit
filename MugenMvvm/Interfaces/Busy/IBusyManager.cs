using System;
using MugenMvvm.Collections;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Busy
{
    public interface IBusyManager : IComponentOwner<IBusyManager>
    {
        IBusyToken? TryBeginBusy(object? request, IReadOnlyMetadataContext? metadata = null);

        IBusyToken? TryGetToken<TState>(TState state, Func<TState, IBusyToken, IReadOnlyMetadataContext?, bool> filter, IReadOnlyMetadataContext? metadata = null);

        ItemOrIReadOnlyList<IBusyToken> GetTokens(IReadOnlyMetadataContext? metadata = null);
    }
}