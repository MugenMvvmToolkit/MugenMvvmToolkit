using System.Collections.Generic;
using MugenMvvm.Delegates;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Interfaces.Busy
{
    public interface IBusyManager : IComponentOwner<IBusyManager>, ISuspendable
    {
        IBusyToken BeginBusy<TRequest>(in TRequest request, IReadOnlyMetadataContext? metadata = null);

        IBusyToken? TryGetToken<TState>(FuncIn<TState, IBusyToken, IReadOnlyMetadataContext?, bool> filter, in TState state, IReadOnlyMetadataContext? metadata = null);

        IReadOnlyList<IBusyToken> GetTokens(IReadOnlyMetadataContext? metadata = null);
    }
}