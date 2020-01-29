﻿using System.Collections.Generic;
using MugenMvvm.Delegates;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Interfaces.Busy.Components
{
    public interface IBusyManagerComponent : IComponent<IBusyManager>, ISuspendable
    {
        IBusyToken? TryBeginBusy<TRequest>(in TRequest request, IReadOnlyMetadataContext? metadata);

        IBusyToken? TryGetToken<TState>(in TState state, FuncIn<TState, IBusyToken, IReadOnlyMetadataContext?, bool> filter, IReadOnlyMetadataContext? metadata);

        IReadOnlyList<IBusyToken>? TryGetTokens(IReadOnlyMetadataContext? metadata);
    }
}