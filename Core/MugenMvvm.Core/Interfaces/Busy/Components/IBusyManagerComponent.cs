using System.Collections.Generic;
using MugenMvvm.Delegates;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Interfaces.Busy.Components
{
    public interface IBusyManagerComponent : IComponent<IBusyManager>, ISuspendable
    {
        IBusyToken? TryBeginBusy<TRequest>(in TRequest request, IReadOnlyMetadataContext? metadata);

        IBusyToken? TryGetToken<TState>(FuncIn<TState, IBusyToken, IReadOnlyMetadataContext?, bool> filter, in TState state, IReadOnlyMetadataContext? metadata);

        IReadOnlyList<IBusyToken>? TryGetTokens(IReadOnlyMetadataContext? metadata);
    }
}