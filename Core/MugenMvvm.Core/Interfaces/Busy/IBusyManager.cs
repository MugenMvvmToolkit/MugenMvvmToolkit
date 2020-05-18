using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Interfaces.Busy
{
    public interface IBusyManager : IComponentOwner<IBusyManager>, ISuspendable
    {
        IBusyToken BeginBusy<TRequest>([AllowNull]in TRequest request, IReadOnlyMetadataContext? metadata = null);

        IBusyToken? TryGetToken<TState>([AllowNull]in TState state, Func<TState, IBusyToken, IReadOnlyMetadataContext?, bool> filter, IReadOnlyMetadataContext? metadata = null);

        IReadOnlyList<IBusyToken> GetTokens(IReadOnlyMetadataContext? metadata = null);
    }
}