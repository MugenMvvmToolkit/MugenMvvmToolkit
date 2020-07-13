using System;
using System.Collections.Generic;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Interfaces.Busy.Components
{
    public interface IBusyManagerComponent : IComponent<IBusyManager>
    {
        IBusyToken? TryBeginBusy(IBusyManager busyManager, object? request, IReadOnlyMetadataContext? metadata);

        IBusyToken? TryGetToken(IBusyManager busyManager, Func<object?, IBusyToken, IReadOnlyMetadataContext?, bool> filter, object? state, IReadOnlyMetadataContext? metadata);

        ItemOrList<IBusyToken, IReadOnlyList<IBusyToken>> TryGetTokens(IBusyManager busyManager, IReadOnlyMetadataContext? metadata);
    }
}