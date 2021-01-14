using System;
using MugenMvvm.Collections;
using MugenMvvm.Interfaces.Busy;
using MugenMvvm.Interfaces.Busy.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.UnitTests.Internal.Internal;
using Should;

namespace MugenMvvm.UnitTests.Busy.Internal
{
    public sealed class TestBusyManagerComponent : TestSuspendableComponent, IBusyManagerComponent, IHasPriority
    {
        private readonly IBusyManager? _owner;

        public TestBusyManagerComponent(IBusyManager? owner = null)
        {
            _owner = owner;
        }

        public Func<object?, IReadOnlyMetadataContext?, IBusyToken?>? TryBeginBusy { get; set; }

        public Func<Func<object?, IBusyToken, IReadOnlyMetadataContext?, bool>, object?, IReadOnlyMetadataContext?, IBusyToken?>? TryGetToken { get; set; }

        public Func<IReadOnlyMetadataContext?, ItemOrIReadOnlyList<IBusyToken>>? TryGetTokens { get; set; }

        public int Priority { get; set; }

        IBusyToken? IBusyManagerComponent.TryBeginBusy(IBusyManager busyManager, object? request, IReadOnlyMetadataContext? metadata)
        {
            _owner?.ShouldEqual(busyManager);
            return TryBeginBusy?.Invoke(request, metadata);
        }

        IBusyToken? IBusyManagerComponent.TryGetToken<TState>(IBusyManager busyManager, Func<TState, IBusyToken, IReadOnlyMetadataContext?, bool> filter, TState state,
            IReadOnlyMetadataContext? metadata)
        {
            _owner?.ShouldEqual(busyManager);
            return TryGetToken?.Invoke((o, token, arg3) => filter((TState) o!, token, arg3), state, metadata);
        }

        ItemOrIReadOnlyList<IBusyToken> IBusyManagerComponent.TryGetTokens(IBusyManager busyManager, IReadOnlyMetadataContext? metadata)
        {
            _owner?.ShouldEqual(busyManager);
            return TryGetTokens?.Invoke(metadata) ?? default;
        }
    }
}