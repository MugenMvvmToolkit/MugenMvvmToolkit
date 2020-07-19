﻿using System;
using System.Collections.Generic;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Interfaces.Busy
{
    public interface IBusyManager : IComponentOwner<IBusyManager>
    {
        IBusyToken? TryBeginBusy(object? request, IReadOnlyMetadataContext? metadata = null);

        IBusyToken? TryGetToken<TState>(TState state, Func<TState, IBusyToken, IReadOnlyMetadataContext?, bool> filter, IReadOnlyMetadataContext? metadata = null);

        ItemOrList<IBusyToken, IReadOnlyList<IBusyToken>> GetTokens(IReadOnlyMetadataContext? metadata = null);
    }
}