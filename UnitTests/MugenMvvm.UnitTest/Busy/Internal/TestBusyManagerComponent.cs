using System;
using System.Collections.Generic;
using MugenMvvm.Interfaces.Busy;
using MugenMvvm.Interfaces.Busy.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;
using MugenMvvm.UnitTest.Internal.Internal;

namespace MugenMvvm.UnitTest.Busy.Internal
{
    public sealed class TestBusyManagerComponent : TestSuspendableComponent, IBusyManagerComponent, IHasPriority
    {
        #region Properties

        public Func<IBusyManager, object?, IReadOnlyMetadataContext?, IBusyToken?>? TryBeginBusy { get; set; }

        public Func<IBusyManager, Func<object?, IBusyToken, IReadOnlyMetadataContext?, bool>, object?, IReadOnlyMetadataContext?, IBusyToken?>? TryGetToken { get; set; }

        public Func<IBusyManager, IReadOnlyMetadataContext?, ItemOrList<IBusyToken, IReadOnlyList<IBusyToken>>>? TryGetTokens { get; set; }

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        IBusyToken? IBusyManagerComponent.TryBeginBusy(IBusyManager busyManager, object? request, IReadOnlyMetadataContext? metadata)
        {
            return TryBeginBusy?.Invoke(busyManager, request, metadata);
        }

        IBusyToken? IBusyManagerComponent.TryGetToken(IBusyManager busyManager, Func<object?, IBusyToken, IReadOnlyMetadataContext?, bool> filter, object? state, IReadOnlyMetadataContext? metadata)
        {
            return TryGetToken?.Invoke(busyManager, filter, state, metadata);
        }

        ItemOrList<IBusyToken, IReadOnlyList<IBusyToken>> IBusyManagerComponent.TryGetTokens(IBusyManager busyManager, IReadOnlyMetadataContext? metadata)
        {
            return TryGetTokens?.Invoke(busyManager, metadata) ?? default;
        }

        #endregion
    }
}