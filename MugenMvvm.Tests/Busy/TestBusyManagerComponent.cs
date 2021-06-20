using System;
using MugenMvvm.Collections;
using MugenMvvm.Interfaces.Busy;
using MugenMvvm.Interfaces.Busy.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Tests.Internal;

namespace MugenMvvm.Tests.Busy
{
    public sealed class TestBusyManagerComponent : TestSuspendableComponent<IBusyManager>, IBusyManagerComponent, IHasPriority
    {
        public Func<IBusyManager, object?, IReadOnlyMetadataContext?, IBusyToken?>? TryBeginBusy { get; set; }

        public Func<IBusyManager, Func<object?, IBusyToken, IReadOnlyMetadataContext?, bool>, object?, IReadOnlyMetadataContext?, IBusyToken?>? TryGetToken { get; set; }

        public Func<IBusyManager, IReadOnlyMetadataContext?, ItemOrIReadOnlyList<IBusyToken>>? TryGetTokens { get; set; }

        public int Priority { get; set; }

        IBusyToken? IBusyManagerComponent.TryBeginBusy(IBusyManager busyManager, object? request, IReadOnlyMetadataContext? metadata) =>
            TryBeginBusy?.Invoke(busyManager, request, metadata);

        IBusyToken? IBusyManagerComponent.TryGetToken<TState>(IBusyManager busyManager, Func<TState, IBusyToken, IReadOnlyMetadataContext?, bool> filter, TState state,
            IReadOnlyMetadataContext? metadata) =>
            TryGetToken?.Invoke(busyManager, (o, token, arg3) => filter((TState)o!, token, arg3), state, metadata);

        ItemOrIReadOnlyList<IBusyToken> IBusyManagerComponent.TryGetTokens(IBusyManager busyManager, IReadOnlyMetadataContext? metadata) =>
            TryGetTokens?.Invoke(busyManager, metadata) ?? default;
    }
}