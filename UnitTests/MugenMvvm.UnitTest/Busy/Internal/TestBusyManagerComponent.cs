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

        public Func<IBusyManager, object?, Type, IReadOnlyMetadataContext?, IBusyToken?>? TryBeginBusy { get; set; }

        public Func<IBusyManager, Func<object?, IBusyToken, IReadOnlyMetadataContext?, bool>, object?, Type, Delegate, IReadOnlyMetadataContext?, IBusyToken?>? TryGetToken { get; set; }

        public Func<IBusyManager, IReadOnlyMetadataContext?, ItemOrList<IBusyToken, IReadOnlyList<IBusyToken>>>? TryGetTokens { get; set; }

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        IBusyToken? IBusyManagerComponent.TryBeginBusy<TRequest>(IBusyManager busyManager, in TRequest request, IReadOnlyMetadataContext? metadata)
        {
            return TryBeginBusy?.Invoke(busyManager, request, typeof(TRequest), metadata);
        }

        IBusyToken? IBusyManagerComponent.TryGetToken<TState>(IBusyManager busyManager, in TState state, Func<TState, IBusyToken, IReadOnlyMetadataContext?, bool> filter, IReadOnlyMetadataContext? metadata)
        {
            return TryGetToken?.Invoke(busyManager, (o, token, arg3) => filter((TState)o!, token, arg3), state, typeof(TState), filter, metadata);
        }

        ItemOrList<IBusyToken, IReadOnlyList<IBusyToken>> IBusyManagerComponent.TryGetTokens(IBusyManager busyManager, IReadOnlyMetadataContext? metadata)
        {
            return TryGetTokens?.Invoke(busyManager, metadata) ?? default;
        }

        #endregion
    }
}